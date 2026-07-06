using System.Collections.Concurrent;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using Voxplux.Core.Models;

namespace Voxplux.Core.Audio;
public sealed class AudioMultiplexer : IDisposable
{
    private readonly ConcurrentDictionary<string, DeviceOutputStream> _streams = new();
    private readonly AudioCaptureEngine _captureEngine;
    private WaveFormat? _captureFormat;
    private bool _disposed;
    public int ActiveStreamCount => _streams.Count(s => s.Value.IsPlaying);
    public int TotalStreamCount => _streams.Count;
    public event EventHandler<DeviceOutputStream>? StreamAdded;
    public event EventHandler<string>? StreamRemoved;

    public AudioMultiplexer(AudioCaptureEngine captureEngine)
    {
        _captureEngine = captureEngine;
        _captureEngine.DataCaptured += OnDataCaptured;
    }
    public DeviceOutputStream? AddDevice(MMDevice device)
    {
        if (_disposed) return null;
        if (_streams.ContainsKey(device.ID)) return _streams[device.ID];
        if (_captureFormat is null)
        {
            if (_captureEngine.CaptureFormat is not null)
                _captureFormat = _captureEngine.CaptureFormat;
            else
                _captureFormat = WaveFormat.CreateIeeeFloatWaveFormat(44100, 2);
        }

        var stream = new DeviceOutputStream(device, _captureFormat);

        if (_streams.TryAdd(device.ID, stream))
        {
            stream.Start();
            StreamAdded?.Invoke(this, stream);
            return stream;
        }

        stream.Dispose();
        return null;
    }
    public void RemoveDevice(string deviceId)
    {
        if (_streams.TryRemove(deviceId, out var stream))
        {
            stream.Dispose();
            StreamRemoved?.Invoke(this, deviceId);
        }
    }
    public DeviceOutputStream? GetStream(string deviceId)
    {
        _streams.TryGetValue(deviceId, out var stream);
        return stream;
    }
    public IReadOnlyCollection<DeviceOutputStream> GetAllStreams()
    {
        return _streams.Values.ToList().AsReadOnly();
    }
    private void OnDataCaptured(object? sender, AudioDataEventArgs e)
    {
        _captureFormat ??= _captureEngine.CaptureFormat;
        foreach (var stream in _streams.Values)
        {
            try
            {
                stream.Feed(e.Buffer, e.BytesRecorded);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[Multiplexer] '{stream.DeviceName}' feed hatası: {ex.Message}");
            }
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _captureEngine.DataCaptured -= OnDataCaptured;

        foreach (var stream in _streams.Values)
        {
            try { stream.Dispose(); } catch { }
        }

        _streams.Clear();
    }
}
