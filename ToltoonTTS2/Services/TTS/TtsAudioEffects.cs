using NAudio.Wave;

class VibratoSampleProvider : ISampleProvider
{
    public float Rate { get; set; }
    public float DepthMs { get; set; }

    private readonly ISampleProvider source;
    private readonly float[] delayBuffer;
    private int writePos;

    private double phase;

    public VibratoSampleProvider(ISampleProvider source, float rateHz = 5f, float depthMs = 8f)
    {
        this.source = source;
        this.Rate = rateHz;
        this.DepthMs = depthMs;

        float sampleRate = source.WaveFormat.SampleRate;
        delayBuffer = new float[(int)(sampleRate * source.WaveFormat.Channels * 0.5f)];
    }

    public WaveFormat WaveFormat => source.WaveFormat;

    public int Read(float[] buffer, int offset, int count)
    {
        int read = source.Read(buffer, offset, count);
        int channels = WaveFormat.Channels;
        float sampleRate = WaveFormat.SampleRate;

        float depthSamples = DepthMs * 0.001f * sampleRate;

        for (int n = 0; n < read; n += channels)
        {
            double lfo = Math.Sin(phase);
            phase += 2 * Math.PI * Rate / sampleRate;

            float delay = depthSamples * (float)((lfo + 1) * 0.5);
            float delaySamples = delay * channels;

            float readPosFloat = writePos - delaySamples;
            if (readPosFloat < 0)
                readPosFloat += delayBuffer.Length;

            int index1 = (int)readPosFloat;
            int index2 = (index1 + channels) % delayBuffer.Length;

            float frac = readPosFloat - index1;

            for (int ch = 0; ch < channels; ch++)
            {
                float s1 = delayBuffer[(index1 + ch) % delayBuffer.Length];
                float s2 = delayBuffer[(index2 + ch) % delayBuffer.Length];

                float wet = s1 + (s2 - s1) * frac;
                float dry = buffer[offset + n + ch];

                buffer[offset + n + ch] = wet;

                delayBuffer[(writePos + ch) % delayBuffer.Length] = dry;
            }

            writePos += channels;
            writePos %= delayBuffer.Length;
        }

        return read;
    }
}
