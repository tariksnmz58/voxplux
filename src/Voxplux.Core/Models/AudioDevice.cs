using NAudio.CoreAudioApi;

namespace Voxplux.Core.Models;
public sealed class AudioDevice
{
    public string Id { get; init; } = string.Empty;
    public string FriendlyName { get; init; } = string.Empty;
    public DeviceType Type { get; init; }
    public DeviceState State { get; init; }
    public string IconPath { get; init; } = string.Empty;
    internal MMDevice? MmDevice { get; init; }

    public static AudioDevice FromMmDevice(MMDevice device)
    {
        var type = DetectDeviceType(device);
        return new AudioDevice
        {
            Id = device.ID,
            FriendlyName = device.FriendlyName,
            Type = type,
            State = device.State,
            IconPath = device.IconPath,
            MmDevice = device
        };
    }

    private static DeviceType DetectDeviceType(MMDevice device)
    {
        var name = device.FriendlyName.ToLowerInvariant();
        var id = device.ID.ToLowerInvariant();

        if (name.Contains("bluetooth") || name.Contains("bt") ||
            id.Contains("bthenum") || id.Contains("bluetooth"))
            return DeviceType.Bluetooth;

        if (name.Contains("usb") || id.Contains("usb"))
            return DeviceType.Usb;

        if (name.Contains("hdmi") || name.Contains("displayport"))
            return DeviceType.Hdmi;

        return DeviceType.Wired;
    }
}

public enum DeviceType
{
    Wired,
    Bluetooth,
    Usb,
    Hdmi
}
