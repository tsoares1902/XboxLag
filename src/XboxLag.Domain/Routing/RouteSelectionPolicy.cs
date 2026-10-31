using XboxLag.Domain.Measurements;

namespace XboxLag.Domain.Routing;

/// <summary>Scores comparable measurements; lower scores win and are not improvement percentages.</summary>
public sealed class RouteSelectionPolicy
{
    private readonly double _latencyWeight;
    private readonly double _jitterWeight;
    private readonly double _lossWeight;

    /// <summary>Creates explicit finite nonnegative weights with a positive total.</summary>
    /// <param name="latencyWeight">Relative latency penalty.</param>
    /// <param name="jitterWeight">Relative jitter penalty.</param>
    /// <param name="lossWeight">Relative penalty per packet-loss percentage point.</param>
    /// <exception cref="ArgumentException">Weights are invalid or all zero.</exception>
    public RouteSelectionPolicy(double latencyWeight, double jitterWeight, double lossWeight)
    {
        double[] weights = [latencyWeight, jitterWeight, lossWeight];
        if (weights.Any(w => !double.IsFinite(w) || w < 0) || weights.All(w => w == 0))
        {
            throw new ArgumentException("Weights must be finite, nonnegative and not all zero.", nameof(latencyWeight));
        }
        double maximum = weights.Max();
        _latencyWeight = latencyWeight / maximum;
        _jitterWeight = jitterWeight / maximum;
        _lossWeight = lossWeight / maximum;
    }

    /// <summary>Selects the lowest score, then ordinal route identity; null explicitly means no selection.</summary>
    /// <param name="routes">Distinct candidate routes.</param>
    /// <param name="measurements">Observations; latest matching observation per route is authoritative.</param>
    /// <param name="target">Required final destination and measurement target.</param>
    /// <param name="context">Required compatible measurement conditions.</param>
    /// <returns>The selected route or null when no eligible finite score exists.</returns>
    public Route? Select(IEnumerable<Route> routes, IEnumerable<RouteMeasurement> measurements, RouteEndpoint target, MeasurementContext context)
    {
        ArgumentNullException.ThrowIfNull(routes);
        ArgumentNullException.ThrowIfNull(measurements);
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(context);
        Route[] candidates = routes.ToArray();
        if (candidates.Select(r => r.Id).Distinct().Count() != candidates.Length)
        {
            throw new ArgumentException("Route identities must be unique.", nameof(routes));
        }
        RouteMeasurement[] observations = measurements.Where(m => m.Target == target && m.Context == context).ToArray();
        return candidates.Where(r => r.Status == RouteStatus.Available && r.Destination == target)
            .Select(r => new { Route = r, Latest = observations.Where(m => m.RouteId == r.Id).OrderByDescending(m => m.Timestamp).ToArray() })
            .Where(x => x.Latest.Length > 0 && x.Latest.Count(m => m.Timestamp == x.Latest[0].Timestamp) == 1)
            .Select(x => new { x.Route, Measurement = x.Latest[0] })
            .Where(x => x.Measurement.Status == MeasurementStatus.Success)
            .Select(x => new { x.Route, Score = Score(x.Measurement.Metrics!) })
            .Where(x => double.IsFinite(x.Score))
            .OrderBy(x => x.Score).ThenBy(x => x.Route.Id.Value, StringComparer.Ordinal)
            .Select(x => x.Route).FirstOrDefault();
    }

    private double Score(RouteMetrics metrics)
    {
        return (metrics.LatencyMs * _latencyWeight / 3) + (metrics.JitterMs * _jitterWeight / 3) + (metrics.PacketLossPercent * _lossWeight / 3);
    }
}
