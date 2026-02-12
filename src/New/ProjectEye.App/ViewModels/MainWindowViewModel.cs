using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProjectEye.Core;
using ProjectEye.Core.Abstractions;
using ProjectEye.Core.Models;
using ProjectEye.Core.Services;

namespace ProjectEye.App.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly FocusSessionEngine _engine;
    private readonly IAppConfigStore _appConfigStore;
    private readonly DispatcherTimer _timer;

    public MainWindowViewModel(FocusSessionEngine engine, IAppConfigStore appConfigStore)
    {
        _engine = engine;
        _appConfigStore = appConfigStore;

        _engine.StateChanged += OnStateChanged;

        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _timer.Tick += (_, _) => _engine.Tick();

        OnStateChanged(_engine.Snapshot);
    }

    [ObservableProperty]
    private string _title = "ProjectEye Avalonia Migration - Task B";

    [ObservableProperty]
    private string _status = "Idle";

    [ObservableProperty]
    private string _remainingTime = "00:00";

    [RelayCommand]
    private void StartWork()
    {
        _engine.StartWork();
        EnsureTimerRunning();
    }

    [RelayCommand]
    private void StartTomato()
    {
        _engine.StartTomato();
        EnsureTimerRunning();
    }

    [RelayCommand]
    private void Pause()
    {
        _engine.Pause();
    }

    [RelayCommand]
    private void Resume()
    {
        _engine.ResumeToWork();
        EnsureTimerRunning();
    }

    [RelayCommand]
    private void Skip()
    {
        _engine.Skip();
        EnsureTimerRunning();
    }

    [RelayCommand]
    private async Task SaveDefaultsAsync()
    {
        await _appConfigStore.SaveAsync(new AppConfig());
        Status = "Default config saved";
    }

    private void EnsureTimerRunning()
    {
        if (!_timer.IsEnabled)
        {
            _timer.Start();
        }
    }

    private void OnStateChanged(SessionSnapshot snapshot)
    {
        Status = snapshot.State.ToString();
        RemainingTime = TimeSpan.FromSeconds(snapshot.RemainingSeconds).ToString(@"mm\:ss");

        if (snapshot.State is AppState.Idle or AppState.Paused)
        {
            _timer.Stop();
        }
    }
}
