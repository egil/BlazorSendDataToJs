namespace BlazorApp3;

public sealed record class TimeSeriesData(List<DateTimeOffset> X, List<double> Y)
{
    public string Type { get; } = "scatter";
}