using XboxLag.Domain.Relays;

namespace XboxLag.Domain.Routing;

/// <summary>Describes a path to a destination, independently of measurements.</summary>
public sealed record Route
{
    /// <summary>Creates a path with a consistent relay association.</summary>
    /// <param name="id">Stable path identity.</param>
    /// <param name="destination">Final server or peer endpoint.</param>
    /// <param name="kind">Direct or intermediary path.</param>
    /// <param name="status">Known availability.</param>
    /// <param name="relayId">Required only for relay paths.</param>
    /// <exception cref="ArgumentException">Relay association contradicts the route kind.</exception>
    public Route(RouteId id, RouteEndpoint destination, RouteKind kind, RouteStatus status, RelayId? relayId = null)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(destination);
        if (!Enum.IsDefined(kind)) { throw new ArgumentOutOfRangeException(nameof(kind)); }
        if (!Enum.IsDefined(status)) { throw new ArgumentOutOfRangeException(nameof(status)); }
        if ((kind == RouteKind.Relay) != (relayId is not null)) { throw new ArgumentException("Only relay routes require a relay.", nameof(relayId)); }
        Id = id;
        Destination = destination;
        Kind = kind;
        Status = status;
        RelayId = relayId;
    }
    /// <summary>Gets stable identity.</summary>
    public RouteId Id { get; }
    /// <summary>Gets the final destination, never the relay address.</summary>
    public RouteEndpoint Destination { get; }
    /// <summary>Gets path kind.</summary>
    public RouteKind Kind { get; }
    /// <summary>Gets availability.</summary>
    public RouteStatus Status { get; }
    /// <summary>Gets the intermediary relay identity.</summary>
    public RelayId? RelayId { get; }
}
