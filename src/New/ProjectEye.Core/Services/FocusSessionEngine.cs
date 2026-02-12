using ProjectEye.Core.Abstractions;
using ProjectEye.Core.Models;

namespace ProjectEye.Core.Services;

public sealed class FocusSessionEngine
{
    private readonly IStatisticsStore _statisticsStore;
    private AppConfig _config;

    public FocusSessionEngine(AppConfig config, IStatisticsStore statisticsStore)
    {
        _config = config;
        _statisticsStore = statisticsStore;
        Snapshot = new SessionSnapshot(AppState.Idle, 0, 0, 0);
    }

    public SessionSnapshot Snapshot { get; private set; }

    public event Action<SessionSnapshot>? StateChanged;

    public void UpdateConfig(AppConfig config)
    {
        _config = config;
    }

    public void StartWork() => UpdateSnapshot(AppState.Working, _config.WarnMinutes * 60);

    public void StartTomato() => UpdateSnapshot(AppState.Tomato, _config.TomatoMinutes * 60);

    public void Pause()
    {
        if (Snapshot.State is AppState.Working or AppState.Resting or AppState.Tomato)
        {
            UpdateSnapshot(AppState.Paused, Snapshot.RemainingSeconds);
        }
    }

    public void ResumeToWork()
    {
        if (Snapshot.State == AppState.Paused)
        {
            UpdateSnapshot(AppState.Working, Snapshot.RemainingSeconds);
        }
    }

    public void Skip()
    {
        switch (Snapshot.State)
        {
            case AppState.Working:
            case AppState.Paused:
                UpdateSnapshot(AppState.Resting, _config.RestSeconds);
                break;
            case AppState.Resting:
                UpdateSnapshot(AppState.Working, _config.WarnMinutes * 60);
                break;
            case AppState.Tomato:
                UpdateSnapshot(AppState.Idle, 0, Snapshot.CompletedWorkSessions, Snapshot.CompletedTomatoSessions + 1);
                _ = _statisticsStore.IncrementCompletedTomatoAsync();
                break;
        }
    }

    public void Tick()
    {
        if (Snapshot.State is not (AppState.Working or AppState.Resting or AppState.Tomato))
        {
            return;
        }

        var next = Snapshot.RemainingSeconds - 1;
        if (next > 0)
        {
            UpdateSnapshot(Snapshot.State, next);
            return;
        }

        if (Snapshot.State == AppState.Working)
        {
            _ = _statisticsStore.IncrementCompletedWorkAsync();
            UpdateSnapshot(AppState.Resting, _config.RestSeconds, Snapshot.CompletedWorkSessions + 1, Snapshot.CompletedTomatoSessions);
            return;
        }

        if (Snapshot.State == AppState.Resting)
        {
            UpdateSnapshot(AppState.Working, _config.WarnMinutes * 60);
            return;
        }

        _ = _statisticsStore.IncrementCompletedTomatoAsync();
        UpdateSnapshot(AppState.Idle, 0, Snapshot.CompletedWorkSessions, Snapshot.CompletedTomatoSessions + 1);
    }

    private void UpdateSnapshot(AppState state, int remainingSeconds)
    {
        UpdateSnapshot(state, remainingSeconds, Snapshot.CompletedWorkSessions, Snapshot.CompletedTomatoSessions);
    }

    private void UpdateSnapshot(AppState state, int remainingSeconds, int completedWorkSessions, int completedTomatoSessions)
    {
        Snapshot = new SessionSnapshot(state, Math.Max(remainingSeconds, 0), completedWorkSessions, completedTomatoSessions);
        StateChanged?.Invoke(Snapshot);
    }
}
