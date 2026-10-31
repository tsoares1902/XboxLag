namespace XboxLag.Domain.Routing;

/// <summary>Contains measured route quality; absence is represented separately.</summary>
public sealed record RouteMetrics
{
    /// <summary>Creates validated metrics.</summary>
    /// <param name="latencyMs">Nonnegative latency in milliseconds.</param>
    /// <param name="jitterMs">Nonnegative jitter in milliseconds.</param>
    /// <param name="packetLossPercent">Loss from zero to one hundred.</param>
    public RouteMetrics(double latencyMs, double jitterMs, double packetLossPercent)
    {
        Validate(latencyMs, double.MaxValue, nameof(latencyMs));
        Validate(jitterMs, double.MaxValue, nameof(jitterMs));
        Validate(packetLossPercent, 100, nameof(packetLossPercent));
        LatencyMs = latencyMs;
        JitterMs = jitterMs;
        PacketLossPercent = packetLossPercent;
    }
    /// <summary>Gets measured latency.</summary>
    public double LatencyMs { get; }
    /// <summary>Gets measured jitter.</summary>
    public double JitterMs { get; }
    /// <summary>Gets measured packet loss percentage.</summary>
    public double PacketLossPercent { get; }

    private static void Validate(double value, double maximum, string name)
    {
        if (!double.IsFinite(value) || value < 0 || value > maximum)
        {
            throw new ArgumentOutOfRangeException(name);
        }
    }
}
