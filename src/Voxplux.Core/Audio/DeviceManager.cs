using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;
using Voxplux.Core.Models;

namespace Voxplux.Core.Audio;
public sealed class DeviceManager : IMMNotificationClient, IDisposable
{
    private readonly MMDeviceEnumerator _enumerator;
    private readonly object _lock = new();
    private bool _disposed;
    public event EventHandler<AudioDevice>? DeviceAdded;
    public event EventHandler<string>? DeviceRemoved;
    public event EventHandler<(string deviceId, DeviceState newState)>? DeviceStateChanged;

    public DeviceManager()
    {
        _enumerator = new MMDeviceEnumerator();
        _enumerator.RegisterEndpointNotificationCallback(this);
    }
    public IReadOnlyList<AudioDevice> GetActiveRenderDevices()
    {
        lock (_lock)
        {
            var devices = new List<AudioDevice>();

            try
            {
                var mmDevices = _enumerator.EnumerateAudioEndPoints(
                    DataFlow.Render, DeviceState.Active);

                foreach (var device in mmDevices)
                {
                    try
                    {
                        devices.Add(AudioDevice.FromMmDevice(device));
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(
                            $"[DeviceManager] Cihaz bilgisi alınamadı: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[DeviceManager] Cihaz listeleme hatası: {ex.Message}");
            }

            return devices.AsReadOnly();
        }
    }
    public MMDevice? GetDeviceById(string deviceId)
    {
        try
        {
            return _enumerator.GetDevice(deviceId);
        }
        catch
        {
            return null;
        }
    }
    public AudioDevice? GetDefaultRenderDevice()
    {
        try
        {
            var device = _enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            return AudioDevice.FromMmDevice(device);
        }
        catch
        {
            return null;
        }
    }

    #region IMMNotificationClient Implementation

    public void OnDeviceStateChanged(string deviceId, DeviceState newState)
    {
        DeviceStateChanged?.Invoke(this, (deviceId, newState));

        if (newState == DeviceState.Active)
        {
            try
            {
                var device = _enumerator.GetDevice(deviceId);
                if (device.DataFlow == DataFlow.Render)
                {
                    DeviceAdded?.Invoke(this, AudioDevice.FromMmDevice(device));
                }
            }
            catch {  }
        }
        else if (newState is DeviceState.NotPresent or DeviceState.Unplugged)
        {
            DeviceRemoved?.Invoke(this, deviceId);
        }
    }

    public void OnDeviceAdded(string pwstrDeviceId)
    {
        try
        {
            var device = _enumerator.GetDevice(pwstrDeviceId);
            if (device.DataFlow == DataFlow.Render && device.State == DeviceState.Active)
            {
                DeviceAdded?.Invoke(this, AudioDevice.FromMmDevice(device));
            }
        }
        catch {  }
    }

    public void OnDeviceRemoved(string deviceId)
    {
        DeviceRemoved?.Invoke(this, deviceId);
    }

    public void OnDefaultDeviceChanged(DataFlow flow, Role role, string defaultDeviceId) { }
    public void OnPropertyValueChanged(string pwstrDeviceId, PropertyKey key) { }

    #endregion

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        try
        {
            _enumerator.UnregisterEndpointNotificationCallback(this);
        }
        catch {  }

        _enumerator.Dispose();
    }
}
