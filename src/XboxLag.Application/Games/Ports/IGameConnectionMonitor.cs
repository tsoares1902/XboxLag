using XboxLag.Domain.Games;

namespace XboxLag.Application.Games.Ports;

/// <summary>Observes process-associated flows without inferring gameplay.</summary>
public interface IGameConnectionMonitor
{
    /// <summary>Observes process-associated flows without inferring gameplay.</summary>
    /// <param name="game">Configured game.</param>
    /// <param name="processes">Detected processes to observe.</param>
    /// <param name="cancellationToken">Cancellation request.</param>
    /// <returns>Observed flows, with Unknown classification unless evidence supports another value.</returns>
    Task<IReadOnlyList<TrafficFlow>> ObserveAsync(GameProfile game, IReadOnlyList<GameProcess> processes, CancellationToken cancellationToken);
}
