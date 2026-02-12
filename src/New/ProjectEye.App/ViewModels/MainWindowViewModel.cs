using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ProjectEye.App.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty]
    private string _title = "ProjectEye Avalonia Bootstrap";

    [ObservableProperty]
    private string _status = "Ready";

    [RelayCommand]
    private void Start()
    {
        Status = "Working";
    }

    [RelayCommand]
    private void Pause()
    {
        Status = "Paused";
    }
}
