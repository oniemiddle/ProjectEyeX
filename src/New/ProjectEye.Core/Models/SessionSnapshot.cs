namespace ProjectEye.Core.Models;

public sealed record SessionSnapshot(
    AppState State,
    int RemainingSeconds,
    int CompletedWorkSessions,
    int CompletedTomatoSessions);
