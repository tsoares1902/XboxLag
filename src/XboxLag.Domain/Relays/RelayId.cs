namespace XboxLag.Domain.Relays;

/// <summary>Identifies a relays entity.</summary>
public sealed record RelayId
{
    /// <summary>Creates a stable identifier.</summary>
    /// <param name="value">Nonempty identifier.</param>
    public RelayId(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value.Trim();
    }
    /// <summary>Gets the stable value.</summary>
    public string Value { get; }
}
