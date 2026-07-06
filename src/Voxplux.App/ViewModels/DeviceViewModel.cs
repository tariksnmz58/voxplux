using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Voxplux.Core.Models;

namespace Voxplux.App.ViewModels;
public partial class DeviceViewModel : ObservableObject
{
    [ObservableProperty]
    private string _deviceId = string.Empty;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private DeviceType _deviceType;

    [ObservableProperty]
    private bool _isActive;

    [ObservableProperty]
    private bool _isConnected = true;

    [ObservableProperty]
    private float _volume = 80f;

    [ObservableProperty]
    private bool _isMuted;

    [ObservableProperty]
    private float _bassBoost;

    [ObservableProperty]
    private float _syncDelayMs;

    [ObservableProperty]
    private bool _isSelected;

    [ObservableProperty]
    private EqualizerViewModel _equalizer = new();
    public string DeviceIcon => DeviceType switch
    {
        DeviceType.Bluetooth => "🎧",
        DeviceType.Usb => "🔌",
        DeviceType.Hdmi => "🖥️",
        DeviceType.Wired => "🔊",
        _ => "🎵"
    };
    public string DeviceTypeName => DeviceType switch
    {
        DeviceType.Bluetooth => "Bluetooth",
        DeviceType.Usb => "USB",
        DeviceType.Hdmi => "HDMI",
        DeviceType.Wired => "Kablolu",
        _ => "Bilinmeyen"
    };
    public string ConnectionStatus => IsConnected ? "Bağlı" : "Bağlantı Kesildi";

    partial void OnIsActiveChanged(bool value)
    {
        OnPropertyChanged(nameof(ConnectionStatus));
    }

    partial void OnIsConnectedChanged(bool value)
    {
        OnPropertyChanged(nameof(ConnectionStatus));
    }

    partial void OnDeviceTypeChanged(DeviceType value)
    {
        OnPropertyChanged(nameof(DeviceIcon));
        OnPropertyChanged(nameof(DeviceTypeName));
    }
    public static DeviceViewModel FromModel(AudioDevice device)
    {
        return new DeviceViewModel
        {
            DeviceId = device.Id,
            Name = device.FriendlyName,
            DeviceType = device.Type,
            IsConnected = device.State == NAudio.CoreAudioApi.DeviceState.Active,
            Volume = 80f,
            Equalizer = new EqualizerViewModel()
        };
    }
}
