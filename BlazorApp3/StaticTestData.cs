using System.Text.Json.Serialization;

namespace BlazorApp3;

public static class StaticTestData
{
    public static TimeSeriesData RawData { get; } = GenerateRawData();

    private static TimeSeriesData GenerateRawData()
    {
        var total = 1_000_000;
        var start = DateTimeOffset.UtcNow.AddDays(-100);

        return new TimeSeriesData(
            Enumerable
                .Range(0, total)
                .Select(x => start.AddSeconds(x))
                .ToList(),
            Enumerable
                .Range(0, total)
                .Select(x => Random.Shared.NextDouble() * Random.Shared.Next(1, 10))
                .ToList());
    }
}
