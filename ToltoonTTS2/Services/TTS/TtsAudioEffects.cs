using NAudio.Wave;

class VibratoSampleProvider : ISampleProvider
{
    public float Rate { get; set; }
    public float DepthMs { get; set; }
    public float WetMix { get; set; }

    private readonly ISampleProvider source;
    private readonly float[] delayBuffer;
    private int writePos;

    private double phase;

    public VibratoSampleProvider(ISampleProvider source, float rateHz = 5f, float depthMs = 8f, float wetMix = 0.35f)
    {
        this.source = source;
        this.Rate = rateHz;
        this.DepthMs = depthMs;
        this.WetMix = Math.Clamp(wetMix, 0f, 1f);

        float sampleRate = source.WaveFormat.SampleRate;
        delayBuffer = new float[(int)(sampleRate * source.WaveFormat.Channels * 0.5f)];
    }

    public WaveFormat WaveFormat => source.WaveFormat;

    public static float CalculateRmsNormalizationGain(ISampleProvider provider, float targetRms = 0.15f)
    {
        double sumSquares = 0.0;
        int totalSamples = 0;

        float[] buffer = new float[4096];
        int read;

        int maxSamples = 44100 * 2; // ограничение

        while ((read = provider.Read(buffer, 0, buffer.Length)) > 0)
        {
            for (int i = 0; i < read; i++)
            {
                float s = buffer[i];
                sumSquares += s * s;
            }

            totalSamples += read;

            if (totalSamples >= maxSamples)
                break;
        }

        if (totalSamples == 0)
            return 1f;

        double rms = Math.Sqrt(sumSquares / totalSamples);

        if (rms <= 0.000001)
            return 1f;

        float gain = (float)(targetRms / rms);

        return Math.Min(gain, 10f);
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

                buffer[offset + n + ch] = dry * (1f - WetMix) + wet * WetMix;

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
    private readonly float depth;
    private float smoothedMod = 1f;

    public RobotSampleProvider(ISampleProvider source, float frequency = 30f, float depth = 0.4f)
    {
        this.source = source;
        this.freq = frequency;
        this.depth = Math.Clamp(depth, 0f, 1f);
    }

    public WaveFormat WaveFormat => source.WaveFormat;

    public int Read(float[] buffer, int offset, int count)
    {
        int read = source.Read(buffer, offset, count);
        float sampleRate = WaveFormat.SampleRate;

        for (int i = 0; i < read; i++)
        {
            float carrier = 0.5f + 0.5f * (float)Math.Sin(phase);
            float targetMod = (1f - depth) + depth * carrier;

            // Сглаживаем модуляцию, чтобы не было "щелчков" от резких скачков амплитуды.
            smoothedMod += (targetMod - smoothedMod) * 0.02f;
            buffer[offset + i] *= smoothedMod;

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
    private readonly float mix;

    public DelaySampleProvider(ISampleProvider source, int delayMs, float feedback = 0.4f, float mix = 0.4f)
    {
        this.source = source;
        this.feedback = Math.Clamp(feedback, 0f, 0.95f);
        this.mix = Math.Clamp(mix, 0f, 1f);

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

            dest[offset + n] = input * (1f - mix) + (input + delayed) * mix;
            buffer[position] = input + delayed * feedback;

            position++;
            if (position >= buffer.Length) position = 0;
        }

        return read;
    }
}

class AdaptiveNormalizationSampleProvider : ISampleProvider
{
    private readonly ISampleProvider _source;
    private readonly float _targetRms;
    private readonly float _attackCoeff;
    private readonly float _releaseCoeff;
    private readonly float _minGain;
    private readonly float _maxGain;
    private float _currentGain = 1f;

    public AdaptiveNormalizationSampleProvider(
        ISampleProvider source,
        float targetRms = 0.12f,
        float minGain = 0.55f,
        float maxGain = 2.0f,
        float attackMs = 10f,
        float releaseMs = 150f)
    {
        _source = source;
        _targetRms = targetRms;
        _minGain = minGain;
        _maxGain = maxGain;

        float sampleRate = source.WaveFormat.SampleRate;
        _attackCoeff = CalcCoeff(attackMs, sampleRate);
        _releaseCoeff = CalcCoeff(releaseMs, sampleRate);
    }

    public WaveFormat WaveFormat => _source.WaveFormat;

    public int Read(float[] buffer, int offset, int count)
    {
        int read = _source.Read(buffer, offset, count);
        if (read <= 0)
            return read;

        double sumSquares = 0;
        for (int i = 0; i < read; i++)
        {
            float s = buffer[offset + i];
            sumSquares += s * s;
        }

        float rms = (float)Math.Sqrt(sumSquares / read);
        if (rms < 1e-6f)
            return read;

        float desiredGain = Math.Clamp(_targetRms / rms, _minGain, _maxGain);
        float coeff = desiredGain < _currentGain ? _attackCoeff : _releaseCoeff;
        _currentGain += (desiredGain - _currentGain) * coeff;

        for (int i = 0; i < read; i++)
            buffer[offset + i] *= _currentGain;

        return read;
    }

    private static float CalcCoeff(float timeMs, float sampleRate)
    {
        float samples = Math.Max(1f, timeMs * 0.001f * sampleRate);
        return 1f / samples;
    }
}

class SoftLimiterSampleProvider : ISampleProvider
{
    private readonly ISampleProvider _source;
    private readonly float _threshold;

    public SoftLimiterSampleProvider(ISampleProvider source, float threshold = 0.92f)
    {
        _source = source;
        _threshold = Math.Clamp(threshold, 0.1f, 0.99f);
    }

    public WaveFormat WaveFormat => _source.WaveFormat;

    public int Read(float[] buffer, int offset, int count)
    {
        int read = _source.Read(buffer, offset, count);
        for (int i = 0; i < read; i++)
        {
            float x = buffer[offset + i];
            float abs = Math.Abs(x);
            if (abs <= _threshold)
                continue;

            float sign = Math.Sign(x);
            float over = (abs - _threshold) / (1f - _threshold);
            float compressed = _threshold + (1f - _threshold) * MathF.Tanh(over);
            buffer[offset + i] = sign * compressed;
        }

        return read;
    }
}

class DistortionSampleProvider : ISampleProvider
{
    private readonly ISampleProvider source;
    private readonly float gain;
    private readonly float mix;

    public DistortionSampleProvider(ISampleProvider source, float gain = 5f, float mix = 1f)
    {
        this.source = source;
        this.gain = gain;
        this.mix = Math.Clamp(mix, 0f, 1f);
    }

    public WaveFormat WaveFormat => source.WaveFormat;

    public int Read(float[] buffer, int offset, int count)
    {
        int read = source.Read(buffer, offset, count);

        for (int i = 0; i < read; i++)
        {
            float dry = buffer[offset + i];
            float sample = dry * gain;
            float wet = MathF.Tanh(sample);
            buffer[offset + i] = dry * (1f - mix) + wet * mix;
        }

        return read;
    }
}
