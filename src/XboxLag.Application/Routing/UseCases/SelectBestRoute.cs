using XboxLag.Domain.Measurements;
using XboxLag.Domain.Routing;


namespace XboxLag.Application.Routing.UseCases;

/// <summary>Delegates route selection to pure domain policy without changing networking.</summary>
public sealed class SelectBestRoute
{
    private readonly RouteSelectionPolicy _policy;
    /// <summary>Creates route selection with explicit scoring policy.</summary>
    /// <param name="policy">Domain selection policy.</param>
    public SelectBestRoute(RouteSelectionPolicy policy)
    {
        ArgumentNullException.ThrowIfNull(policy);
        _policy = policy;
    }
    /// <summary>Returns the selected route or null when no comparable candidate exists.</summary>
    /// <param name="routes">Distinct candidate paths.</param>
    /// <param name="measurements">Traceable observations.</param>
    /// <param name="target">Required final target.</param>
    /// <param name="context">Required measurement conditions.</param>
    /// <returns>Selected path or explicit null outcome.</returns>
    public Route? Execute(IEnumerable<Route> routes, IEnumerable<RouteMeasurement> measurements, RouteEndpoint target, MeasurementContext context)
    {
        return _policy.Select(routes, measurements, target, context);
    }
}
