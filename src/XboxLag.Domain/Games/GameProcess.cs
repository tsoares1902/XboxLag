namespace XboxLag.Domain.Games;

/// <summary>Contains platform-neutral process identification metadata.</summary>
public sealed record GameProcess
{
    /// <summary>Creates process metadata.</summary>
    /// <param name="processId">Positive process identifier without a platform handle.</param>
    /// <param name="executableName">Nonblank executable name.</param>
    public GameProcess(int processId, string executableName)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(processId);
        ArgumentException.ThrowIfNullOrWhiteSpace(executableName);
        ProcessId = processId;
        ExecutableName = executableName.Trim();
    }
    /// <summary>Gets operating system process identifier.</summary>
    public int ProcessId { get; }
    /// <summary>Gets executable name.</summary>
    public string ExecutableName { get; }
}
