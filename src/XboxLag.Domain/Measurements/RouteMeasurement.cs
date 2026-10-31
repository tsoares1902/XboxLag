using XboxLag.Domain.Routing;

namespace XboxLag.Domain.Measurements;

/// <summary>Captures a traceable measurement outcome with an explicit target.</summary>
public sealed record RouteMeasurement
{
    /// <summary>Creates a measurement; only success permits and requires metrics.</summary>
    /// <param name="routeId">Measured path identity.</param>
    /// <param name="target">Actual probe target.</param>
    /// <param name="timestamp">Observation time.</param>
    /// <param name="status">Probe outcome.</param>
    /// <param name="context">Probe method and conditions.</param>
    /// <param name="metrics">Required for success and forbidden otherwise.</param>
    /// <exception cref="ArgumentException">Metrics contradict the outcome.</exception>
    public RouteMeasurement(RouteId routeId, RouteEndpoint target, DateTimeOffset timestamp,
        MeasurementStatus status, MeasurementContext context, RouteMetrics? metrics = null)
    {
        ArgumentNullException.ThrowIfNull(routeId);
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(context);
        if (!Enum.IsDefined(status)) { throw new ArgumentOutOfRangeException(nameof(status)); }
        if ((status == MeasurementStatus.Success) != (metrics is not null)) { throw new ArgumentException("Success requires metrics; other outcomes forbid metrics.", nameof(metrics)); }
        RouteId = routeId;
        Target = target;
        Timestamp = timestamp;
        Status = status;
        Context = context;
        Metrics = metrics;
    }
    /// <summary>Gets route identity.</summary>
    public RouteId RouteId { get; }
    /// <summary>Gets actual measured target.</summary>
    public RouteEndpoint Target { get; }
    /// <summary>Gets observation timestamp.</summary>
    public DateTimeOffset Timestamp { get; }
    /// <summary>Gets outcome.</summary>
    public MeasurementStatus Status { get; }
    /// <summary>Gets probe context.</summary>
    public MeasurementContext Context { get; }
    /// <summary>Gets metrics, absent on failure.</summary>
    public RouteMetrics? Metrics { get; }
}
