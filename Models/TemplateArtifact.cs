namespace DeCloud.Shared.Models;

// ============================================================
// Placement: src/Shared/Models/TemplateArtifact.cs
// ============================================================

/// <summary>
/// Classification of the bytes in a <see cref="TemplateArtifact"/>.
/// Consumed by the node-agent artifact cache to set permissions and
/// by cloud-init scripts to determine how to handle the downloaded file.
/// </summary>
public enum ArtifactType
{
    /// <summary>Compiled executable (dht-node, blockstore-node, relay-node).</summary>
    Binary,

    /// <summary>Shell or Python scripts (wg-mesh-enroll.sh, notify-ready.sh).</summary>
    Script,

    /// <summary>HTML/CSS/JS bundles served by the VM's dashboard (dashboard.tar.gz).</summary>
    WebAsset,

    /// <summary>Configuration files (systemd units, nginx.conf, wg0.conf templates).</summary>
    Config,

    /// <summary>General tar.gz archives with mixed content (shared-scripts.tar.gz).</summary>
    Archive,
}

/// <summary>
/// A file artifact attached to a <c>VmTemplate</c>.
///
/// STORAGE: The platform never stores artifact bytes. Authors host bytes at
/// an external URL (GitHub Releases, S3, CDN); the orchestrator stores
/// only metadata (URL, SHA256, size) on <c>VmTemplate.Artifacts[]</c>.
///
/// DELIVERY: The node agent's <c>IArtifactCacheService</c> fetches each
/// artifact once from <see cref="SourceUrl"/>, verifies <see cref="Sha256"/>,
/// and stores it locally. VMs access the cached file over virbr0 via the
/// local node-agent endpoint, not the upstream URL.
///
/// Cloud-init references an artifact as:
/// <code>
///   ${ARTIFACT_URL:name}    → http://192.168.122.1:5100/api/artifacts/{sha256}
///   ${ARTIFACT_SHA256:name} → {sha256}
/// </code>
///
/// IMMUTABILITY: <see cref="Sha256"/> is verified once at template publish
/// time. After publish, the hash is locked — it cannot be changed without
/// bumping <c>VmTemplate.Revision</c>, which triggers re-verification and
/// (for system templates) node-agent re-download.
///
/// SECURITY: Only HTTPS source URLs are accepted. The node agent verifies
/// SHA256 on every download and re-verifies on serve. A mismatch causes the
/// download to be discarded and the prefetch to fail — the node agent will
/// not serve a tampered artifact to a VM.
///
/// PLACED IN SHARED: both the Orchestrator (stores metadata in MongoDB) and
/// the NodeAgent (deserialises artifacts from the local system_template SQLite
/// record in P9) need this type. Placing it in Shared avoids duplication.
/// </summary>
public class TemplateArtifact
{
    /// <summary>
    /// Unique identifier within the template (UUID).
    /// Stable across template edits — only changes if the artifact is removed
    /// and re-added.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Artifact name — the key used in cloud-init variable substitution.
    /// Must be unique within a template.
    ///
    /// Examples: "dht-node", "relay-node", "dashboard-assets", "shared-scripts".
    /// Cloud-init references this as <c>${ARTIFACT_URL:dht-node}</c>.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Human-readable description (optional, for marketplace display).</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// SHA256 hex digest of the artifact bytes. Mandatory. 64 lowercase hex characters.
    ///
    /// Verified by the orchestrator once at publish time by fetching
    /// <see cref="SourceUrl"/> and computing the hash. Rejected if mismatched.
    ///
    /// Verified by the node agent on every download and re-verified on every
    /// serve. A mismatch causes the artifact to be purged from the local cache.
    ///
    /// Immutable for a given <c>VmTemplate.Revision</c>. Changing the bytes
    /// requires the author to update the hash and bump the template revision.
    /// </summary>
    public string Sha256 { get; set; } = string.Empty;

    /// <summary>
    /// Size of the artifact in bytes (informational, for progress reporting).
    /// Not used for verification — SHA256 is the integrity check.
    /// </summary>
    public long SizeBytes { get; set; }

    /// <summary>
    /// Author-controlled URL where the artifact bytes live.
    /// HTTPS required. Only this URL is fetched during download.
    ///
    /// Verified once at publish time. After publish, the URL is immutable
    /// for this template revision — a URL change requires bumping
    /// <c>VmTemplate.Revision</c> (and re-verification).
    ///
    /// Examples: GitHub Releases asset URL, S3 pre-signed URL, CDN URL.
    /// </summary>
    public string SourceUrl { get; set; } = string.Empty;

    /// <summary>
    /// Block Store CID for this artifact (populated when the artifact has been
    /// replicated into the network's block store — Phase 2 roadmap item).
    ///
    /// When set, node agents prefer fetching from the Block Store over
    /// <see cref="SourceUrl"/>. This closes the "author takes down their URL"
    /// availability gap without requiring the platform to host bytes.
    ///
    /// Null until Phase 2 ships.
    /// </summary>
    public string? BlockStoreCid { get; set; }

    /// <summary>
    /// Target CPU architecture for this artifact.
    /// <c>null</c> = architecture-independent (scripts, config files, web assets).
    /// "amd64" or "arm64" for compiled binaries.
    ///
    /// The node agent and orchestrator both filter on this field when building
    /// the variable substitution map for a specific host.
    /// </summary>
    public string? Architecture { get; set; }

    /// <summary>Classification of the artifact bytes.</summary>
    public ArtifactType Type { get; set; }

    /// <summary>UTC timestamp when this artifact was registered on the template.</summary>
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Wallet address (or user ID) of the user who registered this artifact.
    /// Null for system-seeded artifacts.
    /// </summary>
    public string? RegisteredBy { get; set; }
}
