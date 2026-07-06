namespace Voxplux.Core.Models;
public sealed class DeviceSettings
{
    public float Volume { get; set; } = 1.0f;
    public bool IsMuted { get; set; }
    public float BassBoostGain { get; set; }
    public float EqLowGain { get; set; }
    public float EqMidGain { get; set; }
    public float EqHighGain { get; set; }
    public float SyncDelayMs { get; set; }
    public bool IsActive { get; set; }

    public DeviceSettings Clone() => (DeviceSettings)MemberwiseClone();
}
