// DeCloud.Shared/Contracts/ObligationStatePayload.cs
namespace DeCloud.Shared.Contracts;

/// <summary>
/// Carries a single role's identity state across the orchestrator ↔ node boundary.
/// The orchestrator populates this for every obligation where its stored
/// StateVersion is greater than what the node reported in the heartbeat.
/// </summary>
public class ObligationStatePayload
{
    /// <summary>JSON-serialised identity state blob (see ObligationStateBase subclasses).</summary>
    public string StateJson { get; init; } = string.Empty;

    /// <summary>Monotonic version from the orchestrator — used for conflict resolution.</summary>
    public int Version { get; init; }
}