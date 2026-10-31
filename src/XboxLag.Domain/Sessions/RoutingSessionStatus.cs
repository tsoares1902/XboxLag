namespace XboxLag.Domain.Sessions;

/// <summary>Defines routing session status values.</summary>
public enum RoutingSessionStatus
{
    /// <summary>Created.</summary>
    Created,
    /// <summary>Starting.</summary>
    Starting,
    /// <summary>Active.</summary>
    Active,
    /// <summary>Stopping.</summary>
    Stopping,
    /// <summary>Stopped.</summary>
    Stopped,
    /// <summary>Failed.</summary>
    Failed,
}
