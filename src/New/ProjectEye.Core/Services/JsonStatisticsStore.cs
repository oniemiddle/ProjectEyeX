using System.Text.Json;
using ProjectEye.Core.Abstractions;

namespace ProjectEye.Core.Services;

public sealed class JsonStatisticsStore : IStatisticsStore
{
    private readonly string _statisticsPath;

    public JsonStatisticsStore(string statisticsPath)
    {
        _statisticsPath = statisticsPath;
    }

    public Task IncrementCompletedWorkAsync(CancellationToken cancellationToken = default)
    {
        return IncrementAsync("completedWork", cancellationToken);
    }

    public Task IncrementCompletedTomatoAsync(CancellationToken cancellationToken = default)
    {
        return IncrementAsync("completedTomato", cancellationToken);
    }

    private async Task IncrementAsync(string key, CancellationToken cancellationToken)
    {
        Dictionary<string, int> data = new(StringComparer.OrdinalIgnoreCase);

        if (File.Exists(_statisticsPath))
        {
            await using var read = File.OpenRead(_statisticsPath);
            data = await JsonSerializer.DeserializeAsync<Dictionary<string, int>>(read, cancellationToken: cancellationToken)
                   ?? new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        }

        data[key] = data.TryGetValue(key, out var current) ? current + 1 : 1;

        var directory = Path.GetDirectoryName(_statisticsPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var write = File.Create(_statisticsPath);
        await JsonSerializer.SerializeAsync(write, data, cancellationToken: cancellationToken);
    }
}
