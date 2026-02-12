namespace ProjectEye.Core.Abstractions;

public interface IStatisticsStore
{
    Task IncrementCompletedWorkAsync(CancellationToken cancellationToken = default);
    Task IncrementCompletedTomatoAsync(CancellationToken cancellationToken = default);
}
