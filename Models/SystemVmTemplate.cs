namespace DeCloud.Shared.Models;

// ============================================================
// Placement: src/Shared/Models/SystemVmTemplate.cs
// ============================================================

/// <summary>
/// Lightweight deployment specification for a system VM role, pushed from
/// the orchestrator to the node agent and stored in the local
/// <c>system_template</c> SQLite table.
///
/// This is the node-side projection of <c>VmTemplate</c> — it contains
/// only the fields the <c>SystemVmReconciler</c> needs to create a system
/// VM locally (P10), without the marketplace-facing fields (ratings,
/// author info, pricing, etc.) that are irrelevant on the node.
///
/// DELIVERY CHANNEL
/// Pushed at registration (alongside identity state) for any role where
/// the orchestrator's <c>Revision</c> exceeds the node's stored revision.
/// Stale entries are signalled via <c>NodeHeartbeatResponse.SystemTemplatesPending</c>
/// and the node pulls an updated payload on the next heartbeat cycle.
///
/// STORAGE
/// Stored as a JSON blob in <c>system_template</c> (role TEXT PRIMARY KEY).
/// Keyed by canonical role name ("relay" | "dht" | "blockstore").
/// The full JSON is never logged — only role and revision.
///
/// PLACED IN SHARED so both the Orchestrator (generates the payload from
/// <c>VmTemplate</c>) and the NodeAgent (deserialises and uses it) share
/// the same type without duplication.
/// </summary>
public class SystemVmTemplate
{
    /// <summary>
    /// Canonical role name. Matches the SQLite primary key.
    /// "relay" | "dht" | "blockstore"
    /// </summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// MongoDB _id of the VmTemplate document this spec was built from.
    /// Stable across template revisions (revision bumps preserve the _id).
    /// The reconciler uses this to detect template substitution — if the
    /// obligation's TemplateId changes, the running VM was deployed from a
    /// different template and must be redeployed.
    /// </summary>
    public string TemplateId { get; set; } = string.Empty;

    /// <summary>
    /// Monotonic revision counter, taken from <c>VmTemplate.Revision</c>.
    /// The node compares its stored revision against the orchestrator's to
    /// detect drift. Bumped by the orchestrator whenever the cloud-init
    /// content or artifacts change in a way that should trigger re-deployment.
    /// </summary>
    public int Revision { get; set; } = 1;

    /// <summary>
    /// Raw cloud-init YAML template with variable placeholders intact
    /// (e.g., <c>__VM_ID__</c>, <c>${ARTIFACT_URL:dht-node}</c>).
    /// The reconciler substitutes variables at deploy time using
    /// identity state from SQLite + <c>ResolveArtifactVariables</c>.
    /// </summary>
    public string CloudInitContent { get; set; } = string.Empty;

    /// <summary>
    /// Variables declared by this system-VM template (design §2.2).
    /// Mirrors <see cref="VmTemplate.Variables"/>; copied by the orchestrator
    /// when projecting the marketplace template into the lightweight
    /// node-side spec.
    /// </summary>
    public List<TemplateVariable> Variables { get; set; } = new();

    /// <summary>
    /// Artifacts attached to this template. The reconciler calls
    /// <c>IArtifactCacheService.PrefetchAsync</c> for this list immediately
    /// after the template is saved to SQLite, ensuring all binaries and
    /// scripts are cached before the first Create dispatch.
    /// </summary>
    public List<TemplateArtifact> Artifacts { get; set; } = new();

    // ── Resource spec ────────────────────────────────────────────────────

    /// <summary>Number of virtual CPU cores for this system VM.</summary>
    public int VirtualCpuCores { get; set; } = 1;

    /// <summary>Memory allocation in bytes.</summary>
    public long MemoryBytes { get; set; }

    /// <summary>Disk allocation in bytes.</summary>
    public long DiskBytes { get; set; }

    /// <summary>
    /// Base image URL or image ID.
    /// If it is a full HTTPS URL, the node downloads directly.
    /// If it is an image ID (e.g., "debian-12-dht"), the node resolves it
    /// via its local <c>ImageManager.EnsureImageAvailableAsync</c>.
    /// </summary>
    public string BaseImageUrl { get; set; } = string.Empty;

    /// <summary>
    /// Expected SHA256 of the base image, for integrity verification.
    /// Empty string means no verification (used for well-known hosted images).
    /// </summary>
    public string BaseImageHash { get; set; } = string.Empty;

    // ── Service declarations ─────────────────────────────────────────────

    /// <summary>
    /// Per-service readiness declarations. Consumed by the reconciler when
    /// building the <c>VmSpec.Services</c> list so <c>VmReadinessMonitor</c>
    /// knows which ports and paths to probe before marking the VM Healthy.
    ///
    /// Always includes the implicit "System" (cloud-init) entry. Additional
    /// entries cover service-specific readiness (api health, mesh health).
    /// </summary>
    public List<SystemVmServiceDeclaration> Services { get; set; } = new();

    /// <summary>
    /// When this template was last updated by the orchestrator.
    /// Diagnostic only — the reconciler uses <see cref="Revision"/> for
    /// change detection.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// A single service readiness declaration inside a <see cref="SystemVmTemplate"/>.
/// Mirrors the subset of <c>VmServiceStatus</c> the node needs at deploy time.
/// </summary>
public class SystemVmServiceDeclaration
{
    /// <summary>Service display name (e.g., "System", "dht-api", "dht-mesh").</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Port to probe. Null for the implicit "System" cloud-init check.</summary>
    public int? Port { get; set; }

    /// <summary>Protocol (e.g., "tcp", "http"). Null for "System".</summary>
    public string? Protocol { get; set; }

    /// <summary>
    /// Check strategy name matching <c>CheckType</c> enum on the node:
    /// "CloudInitDone" | "HttpGet" | "ExecCommand" | "TcpConnect".
    /// Stored as string to avoid a Shared → Core dependency.
    /// </summary>
    public string CheckType { get; set; } = "CloudInitDone";

    /// <summary>HTTP path to probe for <c>CheckType = HttpGet</c>.</summary>
    public string? HttpPath { get; set; }

    /// <summary>Seconds to wait before marking the service as timed out.</summary>
    public int TimeoutSeconds { get; set; } = 300;
}

/// <summary>
/// Wire payload carrying a single role's system template in the
/// <c>NodeRegistrationResponse</c> and in heartbeat-driven pull responses.
///
/// Parallel to <c>ObligationStatePayload</c> for identity state.
/// </summary>
public class SystemVmTemplatePayload
{
    /// <summary>
    /// MongoDB _id of the source VmTemplate. Stored by the node agent
    /// alongside the template JSON so it can be reported back to the
    /// orchestrator without deserialising the full template.
    /// </summary>
    public string TemplateId { get; init; } = string.Empty;

    /// <summary>
    /// JSON-serialised <see cref="SystemVmTemplate"/>.
    /// The node agent deserialises and stores in the <c>system_template</c>
    /// SQLite table. Never logged — only role and revision are logged.
    /// </summary>
    public string TemplateJson { get; init; } = string.Empty;

    /// <summary>
    /// Monotonic revision from the orchestrator, used for conflict resolution.
    /// Node stores the revision alongside the JSON for fast version-check
    /// without deserialising.
    /// </summary>
    public int Revision { get; init; }
}
