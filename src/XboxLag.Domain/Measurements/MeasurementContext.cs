namespace XboxLag.Domain.Measurements;

/// <summary>Identifies a probe method and compatible experimental conditions.</summary>
public sealed record MeasurementContext
{
    /// <summary>Creates a context; conditions identify protocol, payload, vantage and sampling settings.</summary>
    /// <param name="method">Nonblank probe method identifier.</param>
    /// <param name="conditions">Nonblank compatibility key for experimental conditions.</param>
    public MeasurementContext(string method, string conditions)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(method);
        ArgumentException.ThrowIfNullOrWhiteSpace(conditions);
        Method = method.Trim();
        Conditions = conditions.Trim();
    }
    /// <summary>Gets measurement method.</summary>
    public string Method { get; }
    /// <summary>Gets compatibility key supplied by the measurement provider.</summary>
    public string Conditions { get; }
}
