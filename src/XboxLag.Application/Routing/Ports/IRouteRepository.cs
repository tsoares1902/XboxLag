using XboxLag.Domain.Routing;

namespace XboxLag.Application.Routing.Ports;

/// <summary>Reads candidate paths for a destination.</summary>
public interface IRouteRepository
{
    /// <summary>Reads candidate paths for a destination.</summary>
    /// <param name="destination">Final destination.</param>
    /// <param name="cancellationToken">Cancellation request.</param>
    /// <returns>Known paths, possibly empty.</returns>
    Task<IReadOnlyList<Route>> GetCandidatesAsync(RouteEndpoint destination, CancellationToken cancellationToken);
}
