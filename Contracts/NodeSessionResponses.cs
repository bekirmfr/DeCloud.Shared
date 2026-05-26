// DeCloud.Shared/Contracts/NodeSessionResponses.cs
namespace DeCloud.Shared.Contracts;

/// <summary>Response to POST /api/nodes/{id}/login.</summary>
public class NodeLoginResponse
{
    /// <summary>True if the node is now scheduling-ready.</summary>
    public bool SchedulingReady { get; set; }

    /// <summary>
    /// Non-null when the node's local settings hash does not match the
    /// orchestrator's stored registration state. Login is accepted but
    /// the operator should investigate and re-register if needed.
    /// </summary>
    public string? SettingsDriftWarning { get; set; }
}

/// <summary>Response to POST /api/nodes/{id}/logout.</summary>
public class NodeLogoutResponse
{
    /// <summary>Always false after a successful logout.</summary>
    public bool SchedulingReady { get; set; }

    /// <summary>Count of VMs still running on this node.</summary>
    public int ActiveVmCount { get; set; }
}

/// <summary>
/// Drift detection detail included in the heartbeat response when the node's
/// local settings hash does not match the orchestrator's stored registration hash.
/// </summary>
public class SettingsDriftInfo
{
    public string Message { get; set; } = string.Empty;
    public string ExpectedHash { get; set; } = string.Empty;
    public string ReportedHash { get; set; } = string.Empty;
}