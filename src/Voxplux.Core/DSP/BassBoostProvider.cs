using NAudio.Dsp;
using NAudio.Wave;

namespace Voxplux.Core.DSP;
public sealed class BassBoostProvider : ISampleProvider
{
    private readonly ISampleProvider _source;
    private readonly int _channels;
    private BiQuadFilter[] _filters;
    private float _gain;
    private float _centerFrequency;

    public WaveFormat WaveFormat => _source.WaveFormat;
    public float Gain
    {
        get => _gain;
        set
        {
            _gain = Math.Clamp(value, 0f, 12f);
            UpdateFilters();
        }
    }
    public float CenterFrequency
    {
        get => _centerFrequency;
        set
        {
            _centerFrequency = Math.Clamp(value, 40f, 200f);
            UpdateFilters();
        }
    }

    public BassBoostProvider(ISampleProvider source, float gain = 0f, float centerFrequency = 100f)
    {
        _source = source;
        _channels = source.WaveFormat.Channels;
        _gain = gain;
        _centerFrequency = centerFrequency;
        _filters = CreateFilters();
    }

    public int Read(float[] buffer, int offset, int count)
    {
        int samplesRead = _source.Read(buffer, offset, count);
        if (_gain <= 0.01f) return samplesRead;

        for (int i = 0; i < samplesRead; i++)
        {
            int channel = i % _channels;
            buffer[offset + i] = _filters[channel].Transform(buffer[offset + i]);
        }

        return samplesRead;
    }

    private BiQuadFilter[] CreateFilters()
    {
        var filters = new BiQuadFilter[_channels];
        for (int i = 0; i < _channels; i++)
        {
            filters[i] = BiQuadFilter.PeakingEQ(
                _source.WaveFormat.SampleRate,
                _centerFrequency,
                0.8f, // Q faktörü — geniş bant bas artırma
                _gain);
        }
        return filters;
    }

    private void UpdateFilters()
    {
        for (int i = 0; i < _channels; i++)
        {
            _filters[i].SetPeakingEq(
                _source.WaveFormat.SampleRate,
                _centerFrequency,
                0.8f,
                _gain);
        }
    }
}
