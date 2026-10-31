using XboxLag.Application.Measurements.Ports;
using XboxLag.Domain.Measurements;
using XboxLag.Domain.Routing;

namespace XboxLag.Application.Measurements.UseCases;

/// <summary>Collects traceable probe outcomes without fabricating metrics.</summary>
public sealed class MeasureRoutes
{
    private readonly IRouteProbe _probe;
    /// <summary>Creates a route measurement use case.</summary>
    /// <param name="probe">Measurement output port.</param>
    public MeasureRoutes(IRouteProbe probe)
    {
        ArgumentNullException.ThrowIfNull(probe);
        _probe = probe;
    }
    /// <summary>Measures candidates sequentially and preserves expected failures; cancellation propagates.</summary>
    /// <param name="routes">Candidate paths to the target.</param>
    /// <param name="target">Required measurement target.</param>
    /// <param name="context">Required method and conditions.</param>
    /// <param name="cancellationToken">Cancellation request.</param>
    /// <returns>Successful and failed observations in candidate order.</returns>
    /// <exception cref="InvalidOperationException">The probe response does not match its request.</exception>
    public async Task<IReadOnlyList<RouteMeasurement>> ExecuteAsync(IEnumerable<Route> routes, RouteEndpoint target,
        MeasurementContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(routes);
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(context);
        cancellationToken.ThrowIfCancellationRequested();
        Route[] candidates = routes.ToArray();
        if (candidates.Any(r => r.Destination != target)) { throw new ArgumentException("Candidate destinations must match the target.", nameof(routes)); }
        List<RouteMeasurement> results = [];
        foreach (Route route in candidates)
        {
            cancellationToken.ThrowIfCancellationRequested();
            RouteMeasurement result = await _probe.MeasureAsync(route, target, context, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            if (result.RouteId != route.Id || result.Target != target || result.Context != context)
            {
                throw new InvalidOperationException("The probe returned an observation for a different request.");
            }
            results.Add(result);
        }
        return results.AsReadOnly();
    }
}
