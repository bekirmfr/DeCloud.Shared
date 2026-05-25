namespace DeCloud.Shared.Models;

/// <summary>
/// Per-resource pricing declared by the node operator at registration or via
/// PATCH /api/nodes/me/pricing. Rates must be >= platform floor rates
/// (enforced by the orchestrator). Zero on any field means "use platform default."
///
/// Note: StoragePerMbPerHour (block-replication cost) is platform-set and
/// intentionally absent here — operators cannot override it.
/// </summary>
public class NodePricing
{
    public string Currency { get; set; } = "USDC";
    public decimal CpuPerHour { get; set; }
    public decimal MemoryPerGbPerHour { get; set; }
    public decimal StoragePerGbPerHour { get; set; }

    /// <summary>
    /// Per-GB-per-hour rate for Proxied GPU VRAM reservations.
    /// Applied to spec.GpuVramBytes at billing time.
    /// 0 = use platform default (DefaultGpuVramPerGbPerHour).
    /// </summary>
    public decimal GpuVramPerGbPerHour { get; set; }

    /// <summary>Returns true if the operator has set any custom pricing.</summary>
    public bool HasCustomPricing =>
        CpuPerHour > 0 || MemoryPerGbPerHour > 0 ||
        StoragePerGbPerHour > 0 || GpuVramPerGbPerHour > 0;
}