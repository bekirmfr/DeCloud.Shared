using System.Security.Cryptography;

namespace DeCloud.Shared.Models;

// ============================================================
// Placement: src/Shared/Models/TemplateArtifact.cs
// ============================================================

/// <summary>
/// Classification of the bytes in a <see cref="TemplateArtifact"/>.
/// Consumed by the node-agent artifact cache to set file permissions and
/// by cloud-init scripts to determine how to handle the downloaded file.
/// </summary>
public enum ArtifactType
{
    /// <summary>
    /// Compiled executable (dht-node, blockstore-node, relay-node).
    /// MUST use an HTTPS <see cref="TemplateArtifact.SourceUrl"/> — inline
    /// <c>data:</c> URIs are rejected for binaries at publish time.
    /// Typically architecture-specific (amd64 / arm64).
    /// </summary>
    Binary,

    /// <summary>
    /// Shell, Python, or other scripts (wg-mesh-enroll.sh, notify-ready.sh).
    /// May use HTTPS or <c>data:</c> URI. Architecture-independent.
    /// </summary>
    Script,

    /// <summary>
    /// HTML, CSS, JS files or bundles (dashboard.html, dashboard.tar.gz).
    /// May use HTTPS or <c>data:</c> URI.
    /// </summary>
    WebAsset,

    /// <summary>
    /// Configuration files (systemd units, nginx.conf, wg0.conf templates).
    /// May use HTTPS or <c>data:</c> URI. Typically small — good candidate
    /// for inline attachment.
    /// </summary>
    Config,

    /// <summary>
    /// General tar.gz archives with mixed content (shared-scripts.tar.gz).
    /// May use HTTPS or <c>data:</c> URI if the archive fits within the
    /// per-artifact size limit.
    /// </summary>
    Archive,

    /// <summary>
    /// Image files (PNG, JPEG, SVG, ICO) used by VM dashboards or UIs.
    /// Logos, favicons, background images, icons.
    /// May use HTTPS or <c>data:</c> URI. Architecture-independent.
    /// Good candidate for inline attachment to keep the template self-contained.
    /// </summary>
    Image,
}

/// <summary>
/// A file artifact attached to a <c>VmTemplate</c>.
///
/// TWO STORAGE MODELS
///
/// <b>External artifact</b> (HTTPS <see cref="SourceUrl"/>):
///   Author hosts bytes externally (GitHub Releases, S3, CDN).
///   The platform stores only metadata (URL + SHA256).
///   The node agent's <c>IArtifactCacheService</c> fetches once from
///   <see cref="SourceUrl"/>, verifies SHA256, and caches locally.
///   Required for <see cref="ArtifactType.Binary"/> artifacts.
///   No size limit enforced by the platform.
///
/// <b>Inline attachment</b> (<c>data:</c> URI <see cref="SourceUrl"/>):
///   Author provides bytes directly as a RFC 2397 data URI:
///   <c>data:{mediaType};base64,{base64bytes}</c>
///   The platform stores the bytes inside the template MongoDB document.
///   The node agent decodes inline — no HTTP fetch occurs.
///   Suitable for scripts, configs, images, and small web assets.
///   Limits: <see cref="MaxInlineArtifactBytes"/> per artifact,
///   <see cref="MaxTotalInlineBytes"/> across all inline artifacts per template.
///   <see cref="ArtifactType.Binary"/> is always rejected as inline.
///
/// COMMON PROPERTIES (both models)
///   SHA256 is verified at publish time and re-verified by the node agent
///   on every cache write. A mismatch causes the artifact to be rejected.
///   The hash is immutable for a given <c>VmTemplate.Revision</c>.
///
/// DELIVERY
///   Regardless of storage model, the VM accesses the artifact via the
///   node agent's local cache endpoint over virbr0:
///   <code>
///     ${ARTIFACT_URL:name}    → http://192.168.122.1:5100/api/artifacts/{sha256}
///     ${ARTIFACT_SHA256:name} → {sha256}
///   </code>
///
/// LEGAL POSTURE
///   External artifacts: platform is a directory, not a host. On takedown,
///   de-listing the template is sufficient — no bytes to remove.
///   Inline attachments: bytes are stored inside the template document.
///   The legal character is the same as storing cloud-init YAML — the author
///   provided the content as part of template registration. The size limits
///   are designed to keep inline attachments in the "small configuration
///   and asset" category, not general-purpose file hosting.
///
/// PLACED IN SHARED: both the Orchestrator (validates at publish time) and
/// the NodeAgent (deserialises from local SQLite in P9) need this type.
/// </summary>
public class TemplateArtifact
{
    // ── Size limits for inline (data:) attachments ───────────────────────

    /// <summary>
    /// Maximum size of a single inline (<c>data:</c>) artifact in bytes.
    /// 5 MB — covers logos, icon sets, small image galleries, and rich
    /// single-file dashboards. Compiled binaries are never inline.
    /// </summary>
    public const int MaxInlineArtifactBytes = 5 * 1024 * 1024;  // 5 MB

    /// <summary>
    /// Maximum total size of all inline artifacts per template in bytes.
    /// 10 MB — leaves comfortable headroom within MongoDB's 16 MB document
    /// limit after accounting for cloud-init YAML and template metadata.
    /// </summary>
    public const int MaxTotalInlineBytes = 10 * 1024 * 1024;    // 10 MB

    // ── Identity ─────────────────────────────────────────────────────────

    /// <summary>
    /// Unique identifier within the template (UUID).
    /// Stable across template edits — only changes if the artifact is
    /// removed and re-added.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Artifact name — the key used in cloud-init variable substitution.
    /// Must be unique within a template.
    ///
    /// Examples: "dht-node", "wg-mesh-enroll", "dashboard-logo", "nginx-conf".
    /// Cloud-init references this as <c>${ARTIFACT_URL:wg-mesh-enroll}</c>.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Human-readable description (optional, for marketplace display).</summary>
    public string Description { get; set; } = string.Empty;

    // ── Integrity ─────────────────────────────────────────────────────────

    /// <summary>
    /// SHA256 hex digest of the artifact bytes. 64 lowercase hex characters.
    /// Mandatory for both HTTPS and <c>data:</c> URI artifacts.
    ///
    /// Verified by the orchestrator at publish time.
    /// Verified by the node agent on every cache write and re-verified on serve.
    /// A mismatch causes the artifact to be rejected and the cache entry purged.
    ///
    /// Immutable for a given <c>VmTemplate.Revision</c>.
    /// </summary>
    public string Sha256 { get; set; } = string.Empty;

    /// <summary>
    /// Size of the artifact bytes (decoded, not base64-encoded).
    /// Informational for HTTPS artifacts (progress reporting).
    /// Enforced for inline artifacts (must not exceed
    /// <see cref="MaxInlineArtifactBytes"/>).
    /// </summary>
    public long SizeBytes { get; set; }

    // ── Source ────────────────────────────────────────────────────────────

    /// <summary>
    /// Where the artifact bytes come from. Two supported schemes:
    ///
    /// <b>HTTPS URL</b> (external artifact):
    ///   <c>https://github.com/org/repo/releases/download/v1.0/binary-amd64</c>
    ///   Bytes are fetched by the node agent at prefetch time and cached locally.
    ///   The platform verifies the SHA256 once at publish by fetching this URL.
    ///   The URL is immutable for a given <c>VmTemplate.Revision</c>.
    ///
    /// <b>data: URI</b> (inline attachment):
    ///   <c>data:text/x-sh;base64,{base64bytes}</c>
    ///   <c>data:image/png;base64,{base64bytes}</c>
    ///   <c>data:application/octet-stream;base64,{base64bytes}</c>
    ///   Bytes are stored in the template MongoDB document.
    ///   The node agent decodes inline — no HTTP fetch occurs.
    ///   <see cref="ArtifactType.Binary"/> artifacts CANNOT use data: URIs.
    ///   Per-artifact limit: <see cref="MaxInlineArtifactBytes"/> (5 MB decoded).
    ///   Per-template limit: <see cref="MaxTotalInlineBytes"/> (10 MB decoded).
    /// </summary>
    public string SourceUrl { get; set; } = string.Empty;

    // ── Distribution helpers ──────────────────────────────────────────────

    /// <summary>
    /// Block Store CID once this artifact has been replicated into the
    /// network's block store (Phase 2 roadmap).
    /// When set, node agents prefer fetching from the Block Store over
    /// <see cref="SourceUrl"/>, closing the "author takes down their URL"
    /// availability gap. Null until Phase 2 ships.
    /// </summary>
    public string? BlockStoreCid { get; set; }

    // ── Classification ────────────────────────────────────────────────────

    /// <summary>
    /// Target CPU architecture.
    /// <c>null</c> = architecture-independent (scripts, configs, images, web assets).
    /// <c>"amd64"</c> or <c>"arm64"</c> for compiled binaries.
    /// The node agent and orchestrator filter on this field when building
    /// the artifact variable substitution map for a specific host.
    /// </summary>
    public string? Architecture { get; set; }

    /// <summary>Classification of the artifact bytes.</summary>
    public ArtifactType Type { get; set; }

    // ── Provenance ────────────────────────────────────────────────────────

    /// <summary>UTC timestamp when this artifact was registered on the template.</summary>
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Wallet address or user ID of the registrant.
    /// Null for system-seeded artifacts.
    /// </summary>
    public string? RegisteredBy { get; set; }

    // ── Computed helpers ──────────────────────────────────────────────────

    /// <summary>
    /// True if this artifact uses a <c>data:</c> URI (inline attachment).
    /// False if it uses an HTTPS URL (external artifact).
    /// </summary>
    public bool IsInline =>
        SourceUrl.StartsWith("data:", StringComparison.OrdinalIgnoreCase);

    // ── Artifact factory ─────────────────────────────────────────────────
    // Mirrors SystemVmTemplateSeeder.Artifact for consistency. If a third
    // seeder appears, refactor this and the SHA256 verification into a shared
    // helper class.

    public static TemplateArtifact Artifact(
        string name,
        string description,
        ArtifactType type,
        string sha256,
        string sourceUrl,
        string? arch = null,
        long sizeBytes = 0)
    {
        if (sourceUrl.StartsWith("data:", StringComparison.OrdinalIgnoreCase) &&
            sha256 != "COMPUTE_FROM_FILE")
        {
            var commaIndex = sourceUrl.IndexOf(',');
            if (commaIndex >= 0)
            {
                var bytes = Convert.FromBase64String(sourceUrl[(commaIndex + 1)..].Trim());
                var actual = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
                if (!string.Equals(actual, sha256, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException(
                        $"GeneralVmTemplateSeeder: inline artifact '{name}' SHA256 mismatch. " +
                        $"Expected {sha256[..12]}, actual {actual[..12]}. " +
                        "Run compute-artifact-constants.sh to regenerate constants.");

                sizeBytes = bytes.Length;
            }
        }

        return new TemplateArtifact
        {
            Name = name,
            Description = description,
            Type = type,
            Architecture = arch,
            Sha256 = sha256,
            SizeBytes = sizeBytes,
            SourceUrl = sourceUrl,
            RegisteredAt = DateTime.UtcNow,
            RegisteredBy = "system",
        };
    }
}
