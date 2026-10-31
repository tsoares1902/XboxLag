using XboxLag.Application.Routing.Ports;
using XboxLag.Domain.Sessions;

namespace XboxLag.Application.Sessions.UseCases;

/// <summary>Coordinates activation and retains a failed session for cleanup on failure or cancellation.</summary>
public sealed class StartRoutingSession
{
    private readonly IRoutingTransport _transport;
    /// <summary>Creates session activation.</summary>
    /// <param name="transport">Transport activation port.</param>
    public StartRoutingSession(IRoutingTransport transport)
    {
        ArgumentNullException.ThrowIfNull(transport);
        _transport = transport;
    }
    /// <summary>Activates a validated created session; cancellation propagates and leaves a failed session.</summary>
    /// <param name="session">Validated Created session; retained by the caller for cleanup.</param>
    /// <param name="cancellationToken">Cancellation request.</param>
    /// <returns>Confirmed transport outcome.</returns>
    public async Task<TransportOutcome> ExecuteAsync(RoutingSession session, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(session);
        cancellationToken.ThrowIfCancellationRequested();
        session.TransitionTo(RoutingSessionStatus.Starting);
        try
        {
            TransportOutcome outcome = await _transport.StartAsync(session, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            if (!Enum.IsDefined(outcome)) { throw new InvalidOperationException("Invalid transport outcome."); }
            session.TransitionTo(outcome == TransportOutcome.Succeeded ? RoutingSessionStatus.Active : RoutingSessionStatus.Failed);
            return outcome;
        }
        catch
        {
            session.TransitionTo(RoutingSessionStatus.Failed);
            throw;
        }
    }
}
