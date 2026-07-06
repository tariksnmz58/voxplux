using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Voxplux.Core.Audio;
using Voxplux.Core.Models;

namespace Voxplux.App.ViewModels;
public partial class MainViewModel : ObservableObject, IDisposable
{
    private readonly AudioCaptureEngine _captureEngine;
    private readonly AudioMultiplexer _multiplexer;
    private readonly DeviceManager _deviceManager;
    private readonly DispatcherTimer _statusTimer;
    private bool _disposed;

    [ObservableProperty]
    private ObservableCollection<DeviceViewModel> _devices = [];

    [ObservableProperty]
    private DeviceViewModel? _selectedDevice;

    [ObservableProperty]
    private bool _isCapturing;

    [ObservableProperty]
    private string _statusText = "Hazır — Ses yönlendirmek için bir cihazın anahtarını açın";

    [ObservableProperty]
    private int _activeDeviceCount;

    [ObservableProperty]
    private string _captureButtonText = "▶  Yakalamayı Başlat";

    [ObservableProperty]
    private bool _showWelcome = true;

    public MainViewModel()
    {
        _captureEngine = new AudioCaptureEngine();
        _multiplexer = new AudioMultiplexer(_captureEngine);
        _deviceManager = new DeviceManager();
        _deviceManager.DeviceAdded += OnDeviceAdded;
        _deviceManager.DeviceRemoved += OnDeviceRemoved;
        _statusTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(500)
        };
        _statusTimer.Tick += OnStatusTimerTick;
        _statusTimer.Start();
        LoadDevices();
    }
    [RelayCommand]
    private void RefreshDevices()
    {
        LoadDevices();
        StatusText = $"🔄 {Devices.Count} cihaz bulundu";
    }
    [RelayCommand]
    private void ToggleCapture()
    {
        if (IsCapturing)
        {
            StopCapture();
        }
        else
        {
            StartCapture();
        }
    }
    public void HandleDeviceToggled(DeviceViewModel device)
    {

        if (device.IsActive)
        {
            if (!IsCapturing)
            {
                StartCapture();
            }
            ActivateDevice(device);
        }
        else
        {
            DeactivateDevice(device);
            if (!Devices.Any(d => d.IsActive))
            {
                StopCapture();
            }
        }

        UpdateActiveCount();
    }
    [RelayCommand]
    private void SelectDevice(DeviceViewModel? device)
    {
        if (device is null) return;
        foreach (var d in Devices)
            d.IsSelected = false;

        device.IsSelected = true;
        SelectedDevice = device;
        ShowWelcome = false;
    }
    [RelayCommand]
    private void ApplySettings()
    {
        if (SelectedDevice is null) return;

        var stream = _multiplexer.GetStream(SelectedDevice.DeviceId);
        if (stream is null) return;

        stream.Settings.Volume = SelectedDevice.Volume / 100f;
        stream.Settings.IsMuted = SelectedDevice.IsMuted;
        stream.Settings.BassBoostGain = SelectedDevice.BassBoost;
        stream.Settings.EqLowGain = SelectedDevice.Equalizer.LowGain;
        stream.Settings.EqMidGain = SelectedDevice.Equalizer.MidGain;
        stream.Settings.EqHighGain = SelectedDevice.Equalizer.HighGain;
        stream.Settings.SyncDelayMs = SelectedDevice.SyncDelayMs;
        stream.ApplySettings();
    }
    [RelayCommand]
    private void ResetEqualizer()
    {
        if (SelectedDevice is null) return;
        SelectedDevice.Equalizer.Reset();
        SelectedDevice.BassBoost = 0;
        ApplySettings();
    }

    private void StartCapture()
    {
        if (IsCapturing) return;

        try
        {
            _captureEngine.StartCapture();
            IsCapturing = true;
            CaptureButtonText = "⏹  Yakalamayı Durdur";
            StatusText = "🔴 Ses yakalanıyor — aktif cihazlara yönlendiriliyor...";
            foreach (var device in Devices.Where(d => d.IsActive))
            {
                ActivateDevice(device);
            }
        }
        catch (Exception ex)
        {
            StatusText = $"⚠️ Hata: {ex.Message}";
            IsCapturing = false;
        }
    }

    private void StopCapture()
    {
        if (!IsCapturing) return;

        _captureEngine.StopCapture();
        IsCapturing = false;
        CaptureButtonText = "▶  Yakalamayı Başlat";
        StatusText = "Durduruldu — Ses yönlendirmek için bir cihazın anahtarını açın";
        foreach (var device in Devices)
        {
            _multiplexer.RemoveDevice(device.DeviceId);
        }
    }

    private void ActivateDevice(DeviceViewModel device)
    {
        if (!IsCapturing) return;

        var mmDevice = _deviceManager.GetDeviceById(device.DeviceId);
        if (mmDevice is null)
        {
            StatusText = $"⚠️ Cihaz bulunamadı: {device.Name}";
            device.IsActive = false;
            return;
        }

        try
        {
            var stream = _multiplexer.AddDevice(mmDevice);
            if (stream is not null)
            {
                stream.Settings.Volume = device.Volume / 100f;
                stream.Settings.IsMuted = device.IsMuted;
                stream.Settings.BassBoostGain = device.BassBoost;
                stream.Settings.EqLowGain = device.Equalizer.LowGain;
                stream.Settings.EqMidGain = device.Equalizer.MidGain;
                stream.Settings.EqHighGain = device.Equalizer.HighGain;
                stream.ApplySettings();

                StatusText = $"✅ {device.Name} aktif — ses yönlendiriliyor";
            }
        }
        catch (Exception ex)
        {
            StatusText = $"⚠️ {device.Name} başlatılamadı: {ex.Message}";
            device.IsActive = false;
        }

        UpdateActiveCount();
    }

    private void DeactivateDevice(DeviceViewModel device)
    {
        _multiplexer.RemoveDevice(device.DeviceId);
        StatusText = $"⏸️ {device.Name} pasife alındı";
        UpdateActiveCount();
    }

    private void LoadDevices()
    {
        var activeDevices = _deviceManager.GetActiveRenderDevices();

        Application.Current?.Dispatcher.Invoke(() =>
        {
            Devices.Clear();
            foreach (var device in activeDevices)
            {
                var vm = DeviceViewModel.FromModel(device);
                Devices.Add(vm);
            }
            if (Devices.Count > 0 && SelectedDevice is null)
            {
                SelectDevice(Devices[0]);
            }

            UpdateActiveCount();
        });
    }

    private void OnDeviceAdded(object? sender, AudioDevice device)
    {
        Application.Current?.Dispatcher.Invoke(() =>
        {
            if (Devices.Any(d => d.DeviceId == device.Id)) return;
            Devices.Add(DeviceViewModel.FromModel(device));
            StatusText = $"🔌 Cihaz bağlandı: {device.FriendlyName}";
        });
    }

    private void OnDeviceRemoved(object? sender, string deviceId)
    {
        Application.Current?.Dispatcher.Invoke(() =>
        {
            var device = Devices.FirstOrDefault(d => d.DeviceId == deviceId);
            if (device is not null)
            {
                _multiplexer.RemoveDevice(deviceId);
                Devices.Remove(device);

                if (SelectedDevice?.DeviceId == deviceId)
                    SelectedDevice = Devices.FirstOrDefault();

                StatusText = $"🔌 Cihaz çıkarıldı: {device.Name}";
            }
            UpdateActiveCount();
        });
    }

    private void UpdateActiveCount()
    {
        ActiveDeviceCount = Devices.Count(d => d.IsActive);
    }

    private void OnStatusTimerTick(object? sender, EventArgs e)
    {
        if (IsCapturing && ActiveDeviceCount > 0)
        {
            var activeStreams = _multiplexer.ActiveStreamCount;
            StatusText = $"🔴 Yakalama Aktif  •  {activeStreams}/{Devices.Count} cihaza ses yönlendiriliyor";
        }
    }
    public void OnDeviceSettingChanged()
    {
        ApplySettings();
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _statusTimer.Stop();

        _deviceManager.DeviceAdded -= OnDeviceAdded;
        _deviceManager.DeviceRemoved -= OnDeviceRemoved;

        _multiplexer.Dispose();
        _captureEngine.Dispose();
        _deviceManager.Dispose();
    }
}
