namespace DeCloud.Shared.Models;

/// <summary>
/// Base class for all obligation identity state.
/// Stored in SQLite obligation_state table as a JSON blob (state_json column).
///
/// SECURITY: State may contain private keys. Never log the full JSON blob —
/// only role and version numbers should appear in application logs.
/// </summary>
public abstract class ObligationStateBase
{
    /// <summary>
    /// Monotonic version assigned by the orchestrator.
    /// Conflict resolution: higher version always wins.
    /// </summary>
    public int Version { get; set; }

    /// <summary>UTC timestamp of the last orchestrator write.</summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Identity state for the Relay system VM role.
/// The WireGuard keypair is the mesh identity anchor — preserving it across
/// redeploys lets all mesh peers reconnect without reconfiguration.
/// </summary>
public class RelayObligationState : ObligationStateBase
{
    /// <summary>WireGuard private key (base64). NEVER log.</summary>
    public string WireGuardPrivateKey { get; set; } = string.Empty;

    /// <summary>WireGuard public key derived from the private key. Safe to log.</summary>
    public string WireGuardPublicKey { get; set; } = string.Empty;

    /// <summary>Relay tunnel IP within the mesh (e.g. "10.20.0.1").</summary>
    public string TunnelIp { get; set; } = string.Empty;

    /// <summary>Subnet allocated to CGNAT nodes routed through this relay (e.g. "10.20.1.0/24").</summary>
    public string RelaySubnet { get; set; } = string.Empty;

    /// <summary>Bearer token for CGNAT node authentication against the relay API. NEVER log.</summary>
    public string AuthToken { get; set; } = string.Empty;
}

/// <summary>
/// Identity state for the DHT system VM role.
/// The Ed25519 keypair is the libp2p peer identity. Preserving it across
/// redeploys eliminates cascading DHT routing table updates.
/// </summary>
public class DhtObligationState : ObligationStateBase
{
    /// <summary>Ed25519 private key as base64-encoded bytes. NEVER log.</summary>
    public string Ed25519PrivateKeyBase64 { get; set; } = string.Empty;

    /// <summary>libp2p peer ID derived from the Ed25519 public key. Safe to log.</summary>
    public string PeerId { get; set; } = string.Empty;

    /// <summary>WireGuard private key for mesh connectivity. NEVER log.</summary>
    public string WireGuardPrivateKey { get; set; } = string.Empty;

    /// <summary>WireGuard public key derived from the private key. Safe to log.</summary>
    public string WireGuardPublicKey { get; set; } = string.Empty;

    /// <summary>WireGuard tunnel IP within the mesh (e.g. "10.30.0.x").</summary>
    public string TunnelIp { get; set; } = string.Empty;

    /// <summary>Bearer token for DHT API authentication. NEVER log.</summary>
    public string AuthToken { get; set; } = string.Empty;
}

/// <summary>
/// Identity state for the BlockStore system VM role.
/// Like DHT, the Ed25519 keypair is the libp2p peer identity used in Kademlia routing.
/// </summary>
public class BlockStoreObligationState : ObligationStateBase
{
    /// <summary>Ed25519 private key as base64-encoded bytes. NEVER log.</summary>
    public string Ed25519PrivateKeyBase64 { get; set; } = string.Empty;

    /// <summary>libp2p peer ID derived from the Ed25519 public key. Safe to log.</summary>
    public string PeerId { get; set; } = string.Empty;

    /// <summary>WireGuard private key for mesh connectivity. NEVER log.</summary>
    public string WireGuardPrivateKey { get; set; } = string.Empty;

    /// <summary>WireGuard public key derived from the private key. Safe to log.</summary>
    public string WireGuardPublicKey { get; set; } = string.Empty;

    /// <summary>WireGuard tunnel IP within the mesh (e.g. "10.40.0.x").</summary>
    public string TunnelIp { get; set; } = string.Empty;

    /// <summary>Bearer token for block store API authentication. NEVER log.</summary>
    public string AuthToken { get; set; } = string.Empty;

    /// <summary>
    /// Storage quota in bytes this node's block store may consume.
    /// Typically 5% of total node storage, calculated at registration time.
    /// </summary>
    public long StorageQuotaBytes { get; set; }
}

/// <summary>
/// Canonical role names and role-related lookups used by both the
/// orchestrator and the node agent.
///
/// This is the single source of truth for string role names. The Orchestrator
/// additionally has <c>SystemVmRoleMap</c> for mappings that require the
/// <c>SystemVmRole</c> enum (which is not available in Shared).
///
/// <b>String-only by design.</b> Adding the <c>SystemVmRole</c> enum here
/// would pull Orchestrator types into Shared, reversing the dependency direction.
/// </summary>
public static class ObligationRole
{
    // ── Canonical names ──────────────────────────────────────────────────

    public const string Relay = "relay";
    public const string Dht = "dht";
    public const string BlockStore = "blockstore";

    // ── Canonical set ────────────────────────────────────────────────────

    /// <summary>
    /// The complete set of system VM role names in dependency order:
    /// Relay (no deps) → Dht (depends on Relay) → BlockStore (depends on Dht).
    ///
    /// Used anywhere code needs to iterate all roles, e.g.:
    ///   • <c>SystemVmReconciler</c> iterates this to evaluate all matrix cells.
    ///   • <c>OrchestratorClient.BuildObligationStateVersionsAsync</c> reports
    ///     versions for all roles.
    ///   • <c>IObligationStateService.GetSystemTemplateRevisionsAsync</c>
    ///     queries all three rows.
    ///
    /// Ordering is significant for diagnostics and heartbeat logs but not
    /// for correctness — dependency enforcement is data-driven via
    /// <see cref="ObligationDescriptor.Deps"/>.
    /// </summary>
    public static readonly IReadOnlyList<string> All =
        [Relay, Dht, BlockStore];

    // ── Canonicalisation ─────────────────────────────────────────────────

    /// <summary>
    /// Normalise an arbitrary role string to its lower-case canonical form,
    /// or return <c>null</c> if unrecognised.
    ///
    /// The gate for all role strings entering the system — call this before
    /// any database access, switch statement, or dictionary lookup keyed on
    /// role name. Prevents open-ended queries and case-sensitivity bugs.
    /// </summary>
    public static string? Canonicalise(string? role) =>
        role?.Trim().ToLowerInvariant() switch
        {
            Relay => Relay,
            Dht => Dht,
            BlockStore => BlockStore,
            _ => null,
        };

    // ── System template slugs ────────────────────────────────────────────

    /// <summary>
    /// Maps a canonical role name to its system VM template slug in MongoDB.
    /// These slugs are how <c>DataStore.GetTemplateBySlugAsync</c> is called
    /// when building the system template payload for the registration response.
    ///
    /// Used on both sides (orchestrator builds, node agent may log).
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown for unrecognised role names. Callers should canonicalise first.
    /// </exception>
    public static string ToTemplateSlug(string canonicalRole) => canonicalRole switch
    {
        Relay => "system-relay",
        Dht => "system-dht",
        BlockStore => "system-blockstore",
        _ => throw new ArgumentOutOfRangeException(
                          nameof(canonicalRole),
                          $"No template slug for role '{canonicalRole}'.")
    };
}

