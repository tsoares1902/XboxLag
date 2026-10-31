using XboxLag.Domain.Routing;

namespace XboxLag.Domain.Relays;

/// <summary>Describes an intermediary without credentials.</summary>
public sealed class RelayNode
{
    /// <summary>Creates a relay node.</summary>
    /// <param name="id">Stable intermediary identity.</param>
    /// <param name="endpoint">Intermediary host and port.</param>
    /// <param name="displayName">Nonblank display name.</param>
    /// <param name="status">Known availability.</param>
    /// <param name="region">Optional nonblank region.</param>
    public RelayNode(RelayId id, RouteEndpoint endpoint, string displayName, RelayStatus status, string? region = null)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(endpoint);
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        if (!Enum.IsDefined(status)) { throw new ArgumentOutOfRangeException(nameof(status)); }
        if (region is not null) { ArgumentException.ThrowIfNullOrWhiteSpace(region); }
        Id = id;
        Endpoint = endpoint;
        DisplayName = displayName.Trim();
        Status = status;
        Region = region?.Trim();
    }
    /// <summary>Gets identity.</summary>
    public RelayId Id { get; }
    /// <summary>Gets the relay endpoint.</summary>
    public RouteEndpoint Endpoint { get; }
    /// <summary>Gets display name.</summary>
    public string DisplayName { get; }
    /// <summary>Gets optional region.</summary>
    public string? Region { get; }
    /// <summary>Gets availability.</summary>
    public RelayStatus Status { get; }
}
