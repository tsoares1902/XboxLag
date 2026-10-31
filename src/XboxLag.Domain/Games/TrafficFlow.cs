using XboxLag.Domain.Routing;

namespace XboxLag.Domain.Games;

/// <summary>Describes an observed flow; process association alone does not imply gameplay.</summary>
public sealed record TrafficFlow
{
    /// <summary>Creates an observation with conservative default classification.</summary>
    /// <param name="protocol">TCP or UDP.</param>
    /// <param name="localEndpoint">Observed local address.</param>
    /// <param name="remoteEndpoint">Observed remote address.</param>
    /// <param name="gameId">Optional game association.</param>
    /// <param name="process">Optional process metadata.</param>
    /// <param name="classification">Evidence-based classification, conservatively Unknown by default.</param>
    public TrafficFlow(TransportProtocol protocol, RouteEndpoint localEndpoint, RouteEndpoint remoteEndpoint,
        GameId? gameId = null, GameProcess? process = null, TrafficClassification classification = TrafficClassification.Unknown)
    {
        ArgumentNullException.ThrowIfNull(localEndpoint);
        ArgumentNullException.ThrowIfNull(remoteEndpoint);
        if (!Enum.IsDefined(protocol)) { throw new ArgumentOutOfRangeException(nameof(protocol)); }
        if (!Enum.IsDefined(classification)) { throw new ArgumentOutOfRangeException(nameof(classification)); }
        Protocol = protocol;
        LocalEndpoint = localEndpoint;
        RemoteEndpoint = remoteEndpoint;
        GameId = gameId;
        Process = process;
        Classification = classification;
    }
    /// <summary>Gets protocol.</summary>
    public TransportProtocol Protocol { get; }
    /// <summary>Gets local endpoint.</summary>
    public RouteEndpoint LocalEndpoint { get; }
    /// <summary>Gets remote endpoint.</summary>
    public RouteEndpoint RemoteEndpoint { get; }
    /// <summary>Gets optional game association.</summary>
    public GameId? GameId { get; }
    /// <summary>Gets optional process metadata.</summary>
    public GameProcess? Process { get; }
    /// <summary>Gets classification supported by observation evidence.</summary>
    public TrafficClassification Classification { get; }
}
