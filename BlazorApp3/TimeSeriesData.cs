using ProtoBuf;

namespace BlazorApp3;

public sealed record class TimeSeriesData
{
    public DateTime[] X { get; set; } = Array.Empty<DateTime>();

    public double[] Y { get; set; } = Array.Empty<double>();

    public string Type { get; } = "scatter";
}

[ProtoContract]
public class TimeSeriesDataProto(TimeSeriesData data)
{
    [ProtoMember(1)]
    public long[] X { get; } = data.X.Select(dt => new DateTimeOffset(dt).ToUnixTimeSeconds()).ToArray();

    [ProtoMember(2)]
    public double[] Y { get; set; } = data.Y;
}