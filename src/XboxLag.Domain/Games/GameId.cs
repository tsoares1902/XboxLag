namespace XboxLag.Domain.Games;

/// <summary>Identifies a games entity.</summary>
public sealed record GameId
{
    /// <summary>Creates a stable identifier.</summary>
    /// <param name="value">Nonempty identifier.</param>
    public GameId(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value.Trim();
    }
    /// <summary>Gets the stable value.</summary>
    public string Value { get; }
}
