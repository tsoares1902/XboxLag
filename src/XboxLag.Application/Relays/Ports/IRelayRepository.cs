
namespace XboxLag.Application.Relays.Ports;

/// <summary>Reads available relay definitions.</summary>
public interface IRelayRepository
{
    /// <summary>Reads available relay definitions.</summary>
    /// <param name="cancellationToken">Cancellation request.</param>
    /// <returns>Known relay definitions with explicit availability.</returns>
    Task<IReadOnlyList<XboxLag.Domain.Relays.RelayNode>> GetAllAsync(CancellationToken cancellationToken);
}
