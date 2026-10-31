using XboxLag.Domain.Games;
using XboxLag.Domain.Routing;

namespace XboxLag.Domain.Sessions;

/// <summary>Enforces routing lifecycle while keeping transport mechanics outside the domain.</summary>
public sealed class RoutingSession
{
    /// <summary>Creates a session for an available route to the requested destination.</summary>
    /// <param name="id">Nonempty session identity.</param>
    /// <param name="gameId">Configured game identity.</param>
    /// <param name="destination">Final destination.</param>
    /// <param name="selectedRoute">Available path matching the destination.</param>
    /// <param name="createdAt">Creation timestamp.</param>
    /// <exception cref="ArgumentException">Identity or selected path is invalid.</exception>
    public RoutingSession(Guid id, GameId gameId, RouteEndpoint destination, Route selectedRoute, DateTimeOffset createdAt)
    {
        if (id == Guid.Empty) { throw new ArgumentException("Session identity cannot be empty.", nameof(id)); }
        ArgumentNullException.ThrowIfNull(gameId);
        ArgumentNullException.ThrowIfNull(destination);
        ArgumentNullException.ThrowIfNull(selectedRoute);
        if (selectedRoute.Destination != destination || selectedRoute.Status != RouteStatus.Available)
        {
            throw new ArgumentException("The selected route must be available and match the destination.", nameof(selectedRoute));
        }
        Id = id;
        GameId = gameId;
        Destination = destination;
        SelectedRoute = selectedRoute;
        CreatedAt = createdAt;
    }
    /// <summary>Gets identity.</summary>
    public Guid Id { get; }
    /// <summary>Gets game identity.</summary>
    public GameId GameId { get; }
    /// <summary>Gets destination.</summary>
    public RouteEndpoint Destination { get; }
    /// <summary>Gets selected path.</summary>
    public Route SelectedRoute { get; }
    /// <summary>Gets creation timestamp.</summary>
    public DateTimeOffset CreatedAt { get; }
    /// <summary>Gets lifecycle state; failure does not imply transport is stopped.</summary>
    public RoutingSessionStatus Status { get; private set; }
    /// <summary>Moves to a valid next lifecycle state.</summary>
    /// <param name="next">Requested lifecycle state.</param>
    /// <exception cref="InvalidOperationException">The transition is invalid.</exception>
    public void TransitionTo(RoutingSessionStatus next)
    {
        bool valid = (Status, next) switch
        {
            (RoutingSessionStatus.Created, RoutingSessionStatus.Starting) => true,
            (RoutingSessionStatus.Starting, RoutingSessionStatus.Active or RoutingSessionStatus.Failed) => true,
            (RoutingSessionStatus.Active, RoutingSessionStatus.Stopping) => true,
            (RoutingSessionStatus.Failed, RoutingSessionStatus.Stopping) => true,
            (RoutingSessionStatus.Stopping, RoutingSessionStatus.Stopped or RoutingSessionStatus.Failed) => true,
            _ => false,
        };
        if (!valid) { throw new InvalidOperationException($"Cannot transition from {Status} to {next}."); }
        Status = next;
    }
}
