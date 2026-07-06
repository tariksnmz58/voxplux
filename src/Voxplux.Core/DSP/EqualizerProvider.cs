using NAudio.Dsp;
using NAudio.Wave;

namespace Voxplux.Core.DSP;
public sealed class EqualizerProvider : ISampleProvider
{
    private readonly ISampleProvider _source;
    private readonly int _channels;
    private BiQuadFilter[] _lowFilters;
    private BiQuadFilter[] _midFilters;
    private BiQuadFilter[] _highFilters;

    private float _lowGain;
    private float _midGain;
    private float _highGain;
    private const float LowFrequency = 150f;   // Low band merkezi
    private const float MidFrequency = 1000f;   // Mid band merkezi
    private const float HighFrequency = 8000f;  // High band merkezi
    private const float BandQ = 1.0f;            // Bant genişliği Q faktörü

    public WaveFormat WaveFormat => _source.WaveFormat;
    public float LowGain
    {
        get => _lowGain;
        set
        {
            _lowGain = Math.Clamp(value, -12f, 12f);
            UpdateBand(_lowFilters, LowFrequency, _lowGain, isLowShelf: true);
        }
    }
    public float MidGain
    {
        get => _midGain;
        set
        {
            _midGain = Math.Clamp(value, -12f, 12f);
            UpdateBand(_midFilters, MidFrequency, _midGain, isLowShelf: false);
        }
    }
    public float HighGain
    {
        get => _highGain;
        set
        {
            _highGain = Math.Clamp(value, -12f, 12f);
            UpdateBandHighShelf(_highFilters, HighFrequency, _highGain);
        }
    }

    public EqualizerProvider(ISampleProvider source)
    {
        _source = source;
        _channels = source.WaveFormat.Channels;

        _lowFilters = CreateBand(LowFrequency, 0, isLowShelf: true);
        _midFilters = CreateBand(MidFrequency, 0, isLowShelf: false);
        _highFilters = CreateBandHighShelf(HighFrequency, 0);
    }

    public int Read(float[] buffer, int offset, int count)
    {
        int samplesRead = _source.Read(buffer, offset, count);
        bool hasEffect = Math.Abs(_lowGain) > 0.1f ||
                         Math.Abs(_midGain) > 0.1f ||
                         Math.Abs(_highGain) > 0.1f;
        if (!hasEffect) return samplesRead;

        for (int i = 0; i < samplesRead; i++)
        {
            int ch = i % _channels;
            float sample = buffer[offset + i];
            if (Math.Abs(_lowGain) > 0.1f)
                sample = _lowFilters[ch].Transform(sample);
            if (Math.Abs(_midGain) > 0.1f)
                sample = _midFilters[ch].Transform(sample);
            if (Math.Abs(_highGain) > 0.1f)
                sample = _highFilters[ch].Transform(sample);

            buffer[offset + i] = sample;
        }

        return samplesRead;
    }

    private BiQuadFilter[] CreateBand(float frequency, float gain, bool isLowShelf)
    {
        var filters = new BiQuadFilter[_channels];
        for (int i = 0; i < _channels; i++)
        {
            filters[i] = isLowShelf
                ? BiQuadFilter.LowShelf(_source.WaveFormat.SampleRate, frequency, BandQ, gain)
                : BiQuadFilter.PeakingEQ(_source.WaveFormat.SampleRate, frequency, BandQ, gain);
        }
        return filters;
    }

    private BiQuadFilter[] CreateBandHighShelf(float frequency, float gain)
    {
        var filters = new BiQuadFilter[_channels];
        for (int i = 0; i < _channels; i++)
        {
            filters[i] = BiQuadFilter.HighShelf(_source.WaveFormat.SampleRate, frequency, BandQ, gain);
        }
        return filters;
    }

    private void UpdateBand(BiQuadFilter[] filters, float frequency, float gain, bool isLowShelf)
    {
        for (int i = 0; i < _channels; i++)
        {
            if (isLowShelf)
                filters[i] = BiQuadFilter.LowShelf(_source.WaveFormat.SampleRate, frequency, BandQ, gain);
            else
                filters[i].SetPeakingEq(_source.WaveFormat.SampleRate, frequency, BandQ, gain);
        }
    }

    private void UpdateBandHighShelf(BiQuadFilter[] filters, float frequency, float gain)
    {
        for (int i = 0; i < _channels; i++)
        {
            filters[i] = BiQuadFilter.HighShelf(_source.WaveFormat.SampleRate, frequency, BandQ, gain);
        }
    }
}
