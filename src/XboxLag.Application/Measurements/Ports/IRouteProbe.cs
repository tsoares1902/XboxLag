using XboxLag.Domain.Measurements;
using XboxLag.Domain.Routing;

namespace XboxLag.Application.Measurements.Ports;

/// <summary>Measures a path; expected probe failures return failed measurements.</summary>
public interface IRouteProbe
{
    /// <summary>Measures a path; expected probe failures return failed measurements.</summary>
    /// <param name="route">Path to measure.</param>
    /// <param name="target">Actual probe target.</param>
    /// <param name="context">Method and compatible experimental conditions.</param>
    /// <param name="cancellationToken">Cancellation request, propagated as cancellation.</param>
    /// <returns>Traceable success or failure without fabricated metrics.</returns>
    Task<RouteMeasurement> MeasureAsync(Route route, RouteEndpoint target, MeasurementContext context, CancellationToken cancellationToken);
}
