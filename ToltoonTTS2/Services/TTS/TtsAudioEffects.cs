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

    public static float CalculateRmsNormalizationGain(ISampleProvider provider, float targetRms = 0.15f)
    {
        double sumSquares = 0.0;
        long totalSamples = 0;

        float[] buffer = new float[4096];
        int read;

        while ((read = provider.Read(buffer, 0, buffer.Length)) > 0)
        {
            for (int i = 0; i < read; i++)
            {
                float s = buffer[i];
                sumSquares += s * s;
            }
            totalSamples += read;
        }

        if (totalSamples == 0)
            return 1f;

        double rms = Math.Sqrt(sumSquares / totalSamples);

        if (rms <= 0.000001)
            return 1f;

        float gain = (float)(targetRms / rms);

        // ограничение, чтобы не усиливать слишком сильно
        if (gain > 10f)
            gain = 10f;

        return gain;
    }


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

class RobotSampleProvider : ISampleProvider
{
    private readonly ISampleProvider source;
    private double phase;
    private readonly float freq;

    public RobotSampleProvider(ISampleProvider source, float frequency = 30f)
    {
        this.source = source;
        this.freq = frequency;
    }

    public WaveFormat WaveFormat => source.WaveFormat;

    public int Read(float[] buffer, int offset, int count)
    {
        int read = source.Read(buffer, offset, count);
        float sampleRate = WaveFormat.SampleRate;

        for (int i = 0; i < read; i++)
        {
            float mod = (float)Math.Sin(phase);
            buffer[offset + i] *= mod;

            phase += 2 * Math.PI * freq / sampleRate;
        }

        return read;
    }
}

class DelaySampleProvider : ISampleProvider
{
    private readonly ISampleProvider source;
    private readonly float[] buffer;
    private int position;
    private readonly float feedback;

    public DelaySampleProvider(ISampleProvider source, int delayMs, float feedback = 0.4f)
    {
        this.source = source;
        this.feedback = feedback;

        int samples = source.WaveFormat.SampleRate * delayMs / 1000 * source.WaveFormat.Channels;
        buffer = new float[samples];
    }

    public WaveFormat WaveFormat => source.WaveFormat;

    public int Read(float[] dest, int offset, int count)
    {
        int read = source.Read(dest, offset, count);

        for (int n = 0; n < read; n++)
        {
            float delayed = buffer[position];
            float input = dest[offset + n];

            dest[offset + n] = input + delayed;
            buffer[position] = input + delayed * feedback;

            position++;
            if (position >= buffer.Length) position = 0;
        }

        return read;
    }
}

class DistortionSampleProvider : ISampleProvider
{
    private readonly ISampleProvider source;
    private readonly float gain;

    public DistortionSampleProvider(ISampleProvider source, float gain = 5f)
    {
        this.source = source;
        this.gain = gain;
    }

    public WaveFormat WaveFormat => source.WaveFormat;

    public int Read(float[] buffer, int offset, int count)
    {
        int read = source.Read(buffer, offset, count);

        for (int i = 0; i < read; i++)
        {
            float sample = buffer[offset + i] * gain;
            buffer[offset + i] = MathF.Tanh(sample);
        }

        return read;
    }
}
