using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace Voxplux.Core.Audio;
public sealed class AudioCaptureEngine : IDisposable
{
    private WasapiLoopbackCapture? _capture;
    private bool _isCapturing;
    private bool _disposed;
    public event EventHandler<AudioDataEventArgs>? DataCaptured;
    public event EventHandler<bool>? CaptureStateChanged;
    public bool IsCapturing => _isCapturing;
    public WaveFormat? CaptureFormat => _capture?.WaveFormat;
    public void StartCapture()
    {
        if (_isCapturing) return;

        _capture = new WasapiLoopbackCapture
        {
            ShareMode = AudioClientShareMode.Shared
        };

        _capture.DataAvailable += OnDataAvailable;
        _capture.RecordingStopped += OnRecordingStopped;

        _capture.StartRecording();
        _isCapturing = true;
        CaptureStateChanged?.Invoke(this, true);
    }
    public void StopCapture()
    {
        if (!_isCapturing || _capture is null) return;

        _capture.StopRecording();
        _isCapturing = false;
        CaptureStateChanged?.Invoke(this, false);
    }

    private void OnDataAvailable(object? sender, WaveInEventArgs e)
    {
        if (e.BytesRecorded <= 0) return;
        var buffer = new byte[e.BytesRecorded];
        Buffer.BlockCopy(e.Buffer, 0, buffer, 0, e.BytesRecorded);

        DataCaptured?.Invoke(this, new AudioDataEventArgs(buffer, e.BytesRecorded));
    }

    private void OnRecordingStopped(object? sender, StoppedEventArgs e)
    {
        _isCapturing = false;
        CaptureStateChanged?.Invoke(this, false);

        if (e.Exception is not null)
        {
            System.Diagnostics.Debug.WriteLine(
                $"[CaptureEngine] Yakalama hatası: {e.Exception.Message}");
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        StopCapture();

        if (_capture is not null)
        {
            _capture.DataAvailable -= OnDataAvailable;
            _capture.RecordingStopped -= OnRecordingStopped;
            _capture.Dispose();
            _capture = null;
        }
    }
}
public sealed class AudioDataEventArgs : EventArgs
{
    public byte[] Buffer { get; }
    public int BytesRecorded { get; }

    public AudioDataEventArgs(byte[] buffer, int bytesRecorded)
    {
        Buffer = buffer;
        BytesRecorded = bytesRecorded;
    }
}
