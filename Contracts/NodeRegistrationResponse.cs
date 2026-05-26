// DeCloud.Shared/Contracts/NodeRegistrationResponse.cs
namespace DeCloud.Shared.Contracts;

/// <summary>
/// Included in the registration response when a locality change
/// makes existing VMs non-compliant with their placement constraints.
/// </summary>
public class NonCompliantVmInfo
{
    public string VmId { get; set; } = string.Empty;
    public string VmName { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}

/// <summary>
/// Wire response for POST /api/nodes/register.
/// Shared so both the orchestrator (sender) and node agent (receiver)
/// use the same type without a project-reference coupling.
/// </summary>
public record NodeRegistrationResponse(
    string NodeId,
    string ApiKey,
    string OrchestratorWireGuardPublicKey,
    TimeSpan HeartbeatInterval,
    List<NonCompliantVmInfo>? NonCompliantVms = null
);