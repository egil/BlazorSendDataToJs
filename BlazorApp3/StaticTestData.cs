using System.Text.Json.Serialization;

namespace BlazorApp3;

public static class StaticTestData
{
    public static TimeSeriesData RawData { get; } = GenerateRawData(1_000_000);

    public static TimeSeriesData RawDataSmall { get; } = GenerateRawData(1000);

    private static TimeSeriesData GenerateRawData(int total)
    {
        var start = DateTime.UtcNow.AddDays(-100);

        return new TimeSeriesData
        {
            X = Enumerable
                .Range(0, total)
                .Select(x => start.AddSeconds(x))
                .ToArray(),
            Y = Enumerable
                .Range(0, total)
                .Select(x => Random.Shared.NextDouble() * Random.Shared.Next(1, 10))
                .ToArray(),
        };
    }
}
