using CommunityToolkit.Mvvm.ComponentModel;

namespace Voxplux.App.ViewModels;
public partial class EqualizerViewModel : ObservableObject
{
    [ObservableProperty]
    private float _lowGain;

    [ObservableProperty]
    private float _midGain;

    [ObservableProperty]
    private float _highGain;
    public void Reset()
    {
        LowGain = 0;
        MidGain = 0;
        HighGain = 0;
    }
}
