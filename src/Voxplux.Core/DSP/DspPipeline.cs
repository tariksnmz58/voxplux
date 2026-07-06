using NAudio.Wave;
using Voxplux.Core.Models;

namespace Voxplux.Core.DSP;
public sealed class DspPipeline : ISampleProvider, IDisposable
{
    private readonly BufferedWaveProvider _bufferProvider;
    private readonly ISampleProvider _sampleSource;

    public VolumeProvider VolumeStage { get; }
    public BassBoostProvider BassBoostStage { get; }
    public EqualizerProvider EqualizerStage { get; }

    public WaveFormat WaveFormat => EqualizerStage.WaveFormat;

    public DspPipeline(WaveFormat captureFormat)
    {
        _bufferProvider = new BufferedWaveProvider(captureFormat)
        {
            BufferLength = captureFormat.AverageBytesPerSecond / 4,
            DiscardOnBufferOverflow = true,
            ReadFully = true  // Buffer boşken sessizlik gönder, akışı DURDURMA
        };
        _sampleSource = _bufferProvider.ToSampleProvider();
        VolumeStage = new VolumeProvider(_sampleSource);
        BassBoostStage = new BassBoostProvider(VolumeStage);
        EqualizerStage = new EqualizerProvider(BassBoostStage);
    }
    public void Feed(byte[] buffer, int bytesRecorded)
    {
        _bufferProvider.AddSamples(buffer, 0, bytesRecorded);
    }
    public void ApplySettings(DeviceSettings settings)
    {
        VolumeStage.Volume = settings.Volume;
        VolumeStage.IsMuted = settings.IsMuted;
        BassBoostStage.Gain = settings.BassBoostGain;
        EqualizerStage.LowGain = settings.EqLowGain;
        EqualizerStage.MidGain = settings.EqMidGain;
        EqualizerStage.HighGain = settings.EqHighGain;
    }

    public int Read(float[] buffer, int offset, int count)
    {
        return EqualizerStage.Read(buffer, offset, count);
    }

    public void Dispose()
    {
        _bufferProvider.ClearBuffer();
    }
}
