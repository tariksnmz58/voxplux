using NAudio.CoreAudioApi;
using NAudio.Wave;
using Voxplux.Core.DSP;
using Voxplux.Core.Models;

namespace Voxplux.Core.Audio;
public sealed class DeviceOutputStream : IDisposable
{
    private WasapiOut? _output;
    private readonly MMDevice _device;
    private bool _isPlaying;
    private bool _disposed;

    public DspPipeline Pipeline { get; }
    public DeviceSettings Settings { get; }
    public string DeviceId => _device.ID;
    public string DeviceName => _device.FriendlyName;
    public bool IsPlaying => _isPlaying;
    public event EventHandler<StoppedEventArgs>? PlaybackStopped;

    public DeviceOutputStream(MMDevice device, WaveFormat captureFormat)
    {
        _device = device;
        Settings = new DeviceSettings { IsActive = true };
        Pipeline = new DspPipeline(captureFormat);
    }
    public void Start()
    {
        if (_isPlaying || _disposed) return;

        try
        {
            _output = new WasapiOut(_device, AudioClientShareMode.Shared, true, 20);
            _output.PlaybackStopped += OnPlaybackStopped;
            _output.Init(Pipeline);
            _output.Play();
            _isPlaying = true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(
                $"[DeviceOutputStream] '{DeviceName}' başlatılamadı: {ex.Message}");
            CleanupOutput();
        }
    }
    public void Stop()
    {
        if (!_isPlaying) return;

        try
        {
            _output?.Stop();
        }
        catch {  }

        _isPlaying = false;
        CleanupOutput();
    }
    public void Feed(byte[] buffer, int bytesRecorded)
    {
        if (!_isPlaying || !Settings.IsActive) return;
        Pipeline.Feed(buffer, bytesRecorded);
    }
    public void ApplySettings()
    {
        Pipeline.ApplySettings(Settings);
    }

    private void OnPlaybackStopped(object? sender, StoppedEventArgs e)
    {
        _isPlaying = false;
        PlaybackStopped?.Invoke(this, e);

        if (e.Exception is not null)
        {
            System.Diagnostics.Debug.WriteLine(
                $"[DeviceOutputStream] '{DeviceName}' hata: {e.Exception.Message}");
        }
    }

    private void CleanupOutput()
    {
        if (_output is not null)
        {
            _output.PlaybackStopped -= OnPlaybackStopped;
            try { _output.Dispose(); } catch { }
            _output = null;
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        Stop();
        Pipeline.Dispose();
    }
}
