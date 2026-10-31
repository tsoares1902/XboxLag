using XboxLag.Application.Games.Ports;
using XboxLag.Domain.Games;

namespace XboxLag.Application.Games.UseCases;

/// <summary>Discovers configured game connections without promoting unknown flows to gameplay.</summary>
public sealed class DiscoverGameConnections
{
    private readonly IGameProcessDetector _detector;
    private readonly IGameConnectionMonitor _monitor;

    /// <summary>Creates the discovery use case.</summary>
    /// <param name="detector">Process detection port.</param>
    /// <param name="monitor">Connection observation port.</param>
    public DiscoverGameConnections(IGameProcessDetector detector, IGameConnectionMonitor monitor)
    {
        ArgumentNullException.ThrowIfNull(detector);
        ArgumentNullException.ThrowIfNull(monitor);
        _detector = detector;
        _monitor = monitor;
    }
    /// <summary>Returns matching observations, or an empty list when the game is not running.</summary>
    /// <param name="game">Configured game profile.</param>
    /// <param name="cancellationToken">Cancellation request.</param>
    /// <returns>Conservatively classified flows associated with detected processes.</returns>
    public async Task<IReadOnlyList<TrafficFlow>> ExecuteAsync(GameProfile game, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(game);
        cancellationToken.ThrowIfCancellationRequested();
        IReadOnlyList<GameProcess> processes = await _detector.DetectAsync(game, cancellationToken).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        if (processes.Count == 0) { return Array.Empty<TrafficFlow>(); }
        IReadOnlyList<TrafficFlow> flows = await _monitor.ObserveAsync(game, processes, cancellationToken).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        return Array.AsReadOnly(flows.Where(f => (f.GameId is null || f.GameId == game.Id)
            && f.Process is not null && processes.Contains(f.Process)).ToArray());
    }
}
