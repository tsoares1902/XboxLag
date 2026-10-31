using XboxLag.Domain.Games;

namespace XboxLag.Application.Games.Ports;

/// <summary>Detects running processes for a configured game.</summary>
public interface IGameProcessDetector
{
    /// <summary>Detects running processes for a configured game.</summary>
    /// <param name="game">Configured executable identification metadata.</param>
    /// <param name="cancellationToken">Cancellation request.</param>
    /// <returns>Matching processes; empty when the game is not running.</returns>
    Task<IReadOnlyList<GameProcess>> DetectAsync(GameProfile game, CancellationToken cancellationToken);
}
