namespace XboxLag.Domain.Routing;

/// <summary>Identifies a routing entity.</summary>
public sealed record RouteId
{
    /// <summary>Creates a stable identifier.</summary>
    /// <param name="value">Nonempty identifier.</param>
    public RouteId(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value.Trim();
    }
    /// <summary>Gets the stable value.</summary>
    public string Value { get; }
}
