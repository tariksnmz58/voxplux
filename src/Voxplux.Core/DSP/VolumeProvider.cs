using NAudio.Wave;

namespace Voxplux.Core.DSP;
public sealed class VolumeProvider : ISampleProvider
{
    private readonly ISampleProvider _source;
    private float _currentVolume;
    private float _targetVolume;
    private bool _isMuted;
    private const float RampStep = 0.002f; // Her sample'da yapılan yumuşak geçiş adımı

    public WaveFormat WaveFormat => _source.WaveFormat;
    public float Volume
    {
        get => _targetVolume;
        set => _targetVolume = Math.Clamp(value, 0f, 1f);
    }
    public bool IsMuted
    {
        get => _isMuted;
        set => _isMuted = value;
    }

    public VolumeProvider(ISampleProvider source, float initialVolume = 1.0f)
    {
        _source = source;
        _currentVolume = initialVolume;
        _targetVolume = initialVolume;
    }

    public int Read(float[] buffer, int offset, int count)
    {
        int samplesRead = _source.Read(buffer, offset, count);

        for (int i = 0; i < samplesRead; i++)
        {
            float target = _isMuted ? 0f : _targetVolume;
            if (_currentVolume < target)
                _currentVolume = Math.Min(_currentVolume + RampStep, target);
            else if (_currentVolume > target)
                _currentVolume = Math.Max(_currentVolume - RampStep, target);

            buffer[offset + i] *= _currentVolume;
        }

        return samplesRead;
    }
}
