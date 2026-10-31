namespace XboxLag.Domain.Games;

/// <summary>Describes executable identification without assuming traffic purpose.</summary>
public sealed class GameProfile
{
    /// <summary>Creates a game profile with executable names.</summary>
    /// <param name="id">Stable game identity.</param>
    /// <param name="displayName">Nonblank display name.</param>
    /// <param name="executableNames">Nonempty collection of executable identification names.</param>
    /// <param name="connectionMode">Known topology or Unknown.</param>
    public GameProfile(GameId id, string displayName, IEnumerable<string> executableNames, GameConnectionMode connectionMode = GameConnectionMode.Unknown)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        ArgumentNullException.ThrowIfNull(executableNames);
        if (!Enum.IsDefined(connectionMode)) { throw new ArgumentOutOfRangeException(nameof(connectionMode)); }
        string[] names = executableNames.ToArray();
        if (names.Length == 0 || names.Any(string.IsNullOrWhiteSpace)) { throw new ArgumentException("Executable names are required.", nameof(executableNames)); }
        Id = id;
        DisplayName = displayName.Trim();
        ExecutableNames = Array.AsReadOnly(names.Select(n => n.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToArray());
        ConnectionMode = connectionMode;
    }
    /// <summary>Gets identity.</summary>
    public GameId Id { get; }
    /// <summary>Gets display name.</summary>
    public string DisplayName { get; }
    /// <summary>Gets executable identification names.</summary>
    public IReadOnlyList<string> ExecutableNames { get; }
    /// <summary>Gets connection topology.</summary>
    public GameConnectionMode ConnectionMode { get; }
}
