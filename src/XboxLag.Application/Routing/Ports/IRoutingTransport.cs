using XboxLag.Domain.Sessions;

namespace XboxLag.Application.Routing.Ports;

/// <summary>Activates transport; success guarantees it is active.</summary>
public interface IRoutingTransport
{
    /// <summary>Activates transport; success guarantees it is active.</summary>
    /// <param name="session">Session in Starting state.</param>
    /// <param name="cancellationToken">Cancellation request.</param>
    /// <returns>Confirmed success or failure with uncertain transport state.</returns>
    Task<TransportOutcome> StartAsync(RoutingSession session, CancellationToken cancellationToken);
    /// <summary>Stops transport; success guarantees it is inactive.</summary>
    /// <param name="session">Session in Stopping state.</param>
    /// <param name="cancellationToken">Cancellation request.</param>
    /// <returns>Success only after shutdown is confirmed.</returns>
    Task<TransportOutcome> StopAsync(RoutingSession session, CancellationToken cancellationToken);
}
