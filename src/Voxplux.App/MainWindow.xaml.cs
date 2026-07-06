using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Voxplux.App.ViewModels;
using Wpf.Ui.Controls;

namespace Voxplux.App;

public partial class MainWindow : FluentWindow
{
    private readonly MainViewModel _viewModel;

    public MainWindow()
    {
        _viewModel = new MainViewModel();
        DataContext = _viewModel;

        InitializeComponent();
        _viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(MainViewModel.SelectedDevice))
            {
                UpdateControlPanelVisibility();
            }
            else if (e.PropertyName == nameof(MainViewModel.ShowWelcome))
            {
                UpdateControlPanelVisibility();
            }
        };
    }

    private void UpdateControlPanelVisibility()
    {
        if (_viewModel.SelectedDevice is not null && !_viewModel.ShowWelcome)
        {
            WelcomePanel.Visibility = Visibility.Collapsed;
            ControlPanel.Visibility = Visibility.Visible;
        }
        else
        {
            WelcomePanel.Visibility = Visibility.Visible;
            ControlPanel.Visibility = Visibility.Collapsed;
        }
    }

    private void OnToggleCaptureClick(object sender, RoutedEventArgs e)
    {
        _viewModel.ToggleCaptureCommand.Execute(null);
    }

    private void OnRefreshClick(object sender, RoutedEventArgs e)
    {
        _viewModel.RefreshDevicesCommand.Execute(null);
    }

    private void OnDeviceCardClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement element && element.DataContext is DeviceViewModel device)
        {
            _viewModel.SelectDeviceCommand.Execute(device);
        }
    }
    private void OnDeviceToggleChanged(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement element && element.DataContext is DeviceViewModel device)
        {
            _viewModel.HandleDeviceToggled(device);
            if (device.IsActive)
            {
                _viewModel.SelectDeviceCommand.Execute(device);
            }
        }
    }

    private void OnSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        _viewModel?.OnDeviceSettingChanged();
    }

    private void OnResetEqClick(object sender, RoutedEventArgs e)
    {
        _viewModel.ResetEqualizerCommand.Execute(null);
    }

    protected override void OnClosed(EventArgs e)
    {
        _viewModel.Dispose();
        base.OnClosed(e);
    }
}
