// DeCloud.Shared/Contracts/NodeHeartbeat.cs
using DeCloud.Shared.Models;

namespace DeCloud.Shared.Contracts;

/// <summary>
/// Heartbeat body sent from node agent → orchestrator.
/// POST /api/nodes/{id}/heartbeat
///
/// Defined in Shared so the node agent constructs a typed object
/// and the orchestrator deserialises the same type — no anonymous
/// object projection, no silent field-name drift.
/// </summary>
public record NodeHeartbeat(
    string NodeId,
    NodeMetrics Metrics,
    int SchedulingConfigVersion,
    List<HeartbeatVmInfo>? ActiveVms = null,
    CgnatNodeInfo? CgnatInfo = null,
    Dictionary<string, string>? SystemVmBinaryVersions = null,
    Dictionary<string, int>? ObligationStateVersions = null,
    Dictionary<string, string>? ObligationHealth = null,
    Dictionary<string, int>? SystemTemplateVersions = null,
    string? AgentVersion = null,
    string? SettingsHash = null
);

/// <summary>
/// Per-node performance snapshot carried in every heartbeat.
/// </summary>
public class NodeMetrics
{
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public double CpuUsagePercent { get; set; }
    public double MemoryUsagePercent { get; set; }
    public double StorageUsagePercent { get; set; }
    public double NetworkInMbps { get; set; }
    public double NetworkOutMbps { get; set; }
    public int ActiveVmCount { get; set; }
    public double LoadAverage { get; set; }
}

/// <summary>
/// Compact VM snapshot included in each heartbeat.
/// The orchestrator uses this to adopt autonomously-created system VMs
/// and detect VMs running on the wrong node.
/// </summary>
public class HeartbeatVmInfo
{
    public string VmId { get; set; } = string.Empty;
    public string? Name { get; set; }
    /// <summary>"Running", "Stopped", "Deleted", etc. — string to survive future VmState additions.</summary>
    public string State { get; set; } = string.Empty;
    /// <summary>VM role type string — used by orchestrator to adopt system VMs.</summary>
    public string? VmType { get; set; }
    public string OwnerId { get; set; } = string.Empty;
    /// <summary>Binary version from /diagnostics. Non-null only for running Dht/BlockStore VMs.</summary>
    public string? BinaryVersion { get; set; }
    public bool IsIpAssigned { get; set; }
    public string? IpAddress { get; set; }
    public string? MacAddress { get; set; }
    public int? SshPort { get; set; } = 2222;
    public int? VncPort { get; set; }
    public int VirtualCpuCores { get; set; }
    public int QualityTier { get; set; }
    public int ComputePointCost { get; set; }
    public long? MemoryBytes { get; set; }
    public long? DiskBytes { get; set; }
    /// <summary>GPU access mode: 0=None, 1=Passthrough, 2=Proxied.</summary>
    public int GpuMode { get; set; }
    public long? GpuVramBytes { get; set; }
    public string? ImageId { get; set; }
    public DateTime? StartedAt { get; set; }
    public List<HeartbeatServiceInfo>? Services { get; set; }
}

/// <summary>
/// Per-service readiness status carried inside HeartbeatVmInfo.
/// </summary>
public class HeartbeatServiceInfo
{
    public string Name { get; set; } = string.Empty;
    public int? Port { get; set; }
    public string? Protocol { get; set; }
    /// <summary>Pending | Checking | Ready | TimedOut | Failed</summary>
    public string Status { get; set; } = "Pending";
    public string? StatusMessage { get; set; }
    public DateTime? ReadyAt { get; set; }
}