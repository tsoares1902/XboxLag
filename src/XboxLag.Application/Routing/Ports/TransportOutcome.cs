namespace XboxLag.Application.Routing.Ports;

/// <summary>Reports confirmed transport completion or failure with uncertain transport state.</summary>
public enum TransportOutcome
{
    /// <summary>The requested state was confirmed.</summary>
    Succeeded,
    /// <summary>The requested state was not confirmed; cleanup may still be required.</summary>
    Failed,
}
