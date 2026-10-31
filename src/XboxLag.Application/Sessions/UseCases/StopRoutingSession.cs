using XboxLag.Application.Routing.Ports;
using XboxLag.Domain.Sessions;

namespace XboxLag.Application.Sessions.UseCases;

/// <summary>Confirms transport shutdown before marking a session stopped.</summary>
public sealed class StopRoutingSession
{
    private readonly IRoutingTransport _transport;
    /// <summary>Creates session shutdown.</summary>
    /// <param name="transport">Transport shutdown port.</param>
    public StopRoutingSession(IRoutingTransport transport)
    {
        ArgumentNullException.ThrowIfNull(transport);
        _transport = transport;
    }
    /// <summary>Stops an active or failed session; failure and cancellation retain a retryable failed state.</summary>
    /// <param name="session">Active or Failed session owned exclusively by the caller.</param>
    /// <param name="cancellationToken">Cancellation request.</param>
    /// <returns>Success only after confirmed shutdown.</returns>
    public async Task<TransportOutcome> ExecuteAsync(RoutingSession session, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(session);
        cancellationToken.ThrowIfCancellationRequested();
        session.TransitionTo(RoutingSessionStatus.Stopping);
        try
        {
            TransportOutcome outcome = await _transport.StopAsync(session, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            if (!Enum.IsDefined(outcome)) { throw new InvalidOperationException("Invalid transport outcome."); }
            session.TransitionTo(outcome == TransportOutcome.Succeeded ? RoutingSessionStatus.Stopped : RoutingSessionStatus.Failed);
            return outcome;
        }
        catch
        {
            session.TransitionTo(RoutingSessionStatus.Failed);
            throw;
        }
    }
}
