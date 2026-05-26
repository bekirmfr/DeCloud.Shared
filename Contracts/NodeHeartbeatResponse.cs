// DeCloud.Shared/Contracts/NodeHeartbeatResponse.cs
using DeCloud.Shared.Models;

namespace DeCloud.Shared.Contracts;

/// <summary>
/// Orchestrator response to a node heartbeat.
/// POST /api/nodes/{id}/heartbeat → 200 OK with this as the data envelope payload.
///
/// Defined in Shared so the node agent deserialises a typed object
/// instead of navigating a JsonDocument tree — eliminates the class of
/// silent property-name mismatch bugs.
/// </summary>
public record NodeHeartbeatResponse(
    bool Acknowledged,
    List<NodeCommand>? PendingCommands,
    AgentSchedulingConfig? SchedulingConfig,
    CgnatNodeInfo? CgnatInfo,
    /// <summary>
    /// VM IDs the node is running that it should NOT be running.
    /// Node agent must destroy these immediately.
    /// </summary>
    List<string>? InvalidVmIds = null,
    /// <summary>
    /// Role names where the orchestrator has a higher StateVersion than the node
    /// reported. Node must pull each role's state via GET /api/nodes/me/obligations/{role}/state.
    /// </summary>
    List<string>? ObligationStatesPending = null,
    /// <summary>
    /// Role names where the orchestrator has a higher template revision.
    /// Node must pull via GET /api/nodes/me/system-templates/{role}.
    /// </summary>
    List<string>? SystemTemplatesPending = null,
    SettingsDriftInfo? SettingsDrift = null,
    bool SchedulingReady = false
);