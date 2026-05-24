// src/Shared/AllocatedResources.cs
// Operator-configured resource allocation limits.
// Shared between Orchestrator and NodeAgent to ensure wire-compatible serialization.

using System.Text.Json.Serialization;

namespace DeCloud.Shared.Models;

/// <summary>
/// Operator-configured resource allocation limits.
///
/// <para><b>Schema v2 (current):</b> Allocation is expressed as percentages of
/// detected physical capacity. The orchestrator resolves percentages to concrete
/// values at login time using the current performance evaluation. This decouples
/// allocation from evaluation — percentages remain valid across re-benchmarks.</para>
///
/// <para><b>Schema v1 (legacy):</b> Allocation was expressed as absolute values
/// (bytes, points) resolved by the node agent before registration. During the
/// transition window, the orchestrator accepts both formats and converts v1 to
/// v2 by dividing absolute values by physical totals from HardwareInventory.</para>
///
/// <para>Each percentage field is nullable: null means "use platform default"
/// (<see cref="DefaultPercent"/> for continuous resources, all detected for GPU).
/// The orchestrator applies the default when it receives a null field — this
/// preserves backward compatibility with pre-feature agents that do not send
/// this object at all.</para>
///
/// See docs/NODE-LIFECYCLE.md §3 for the full design.
/// </summary>
public class AllocatedResources
{
    // =========================================================================
    // Schema version
    // =========================================================================

    /// <summary>
    /// Format discriminator.
    /// <list type="bullet">
    ///   <item><term>1 (or absent/0)</term><description>Legacy format — absolute values in
    ///     <see cref="MemoryBytes"/>, <see cref="ComputePoints"/>, <see cref="StorageBytes"/>.</description></item>
    ///   <item><term>2</term><description>Current format — percentages in
    ///     <see cref="CpuPercent"/>, <see cref="MemoryPercent"/>, <see cref="StoragePercent"/>.</description></item>
    /// </list>
    /// The orchestrator checks this field to decide which set of properties to read.
    /// </summary>
    public int SchemaVersion { get; set; }

    // =========================================================================
    // Schema v2 — percentage-based allocation (current)
    // =========================================================================

    /// <summary>
    /// Percentage of hardware-max compute points to offer (0.01–0.95).
    /// The orchestrator multiplies this by the evaluated points-per-core × cores
    /// at login time to produce a concrete point budget.
    /// Null = platform default (<see cref="DefaultPercent"/>).
    /// </summary>
    public double? CpuPercent { get; set; }

    /// <summary>
    /// Percentage of physical RAM to offer (0.01–0.95).
    /// Null = platform default (<see cref="DefaultPercent"/>).
    /// </summary>
    public double? MemoryPercent { get; set; }

    /// <summary>
    /// Percentage of physical storage to offer (0.01–0.95).
    /// This is pre-overcommit; the orchestrator applies tier-specific storage
    /// overcommit on top.
    /// Null = platform default (<see cref="DefaultPercent"/>).
    /// </summary>
    public double? StoragePercent { get; set; }

    // =========================================================================
    // Schema v1 — absolute-value allocation (legacy, read-only for transition)
    // =========================================================================

    /// <summary>
    /// [Legacy v1] Operator-allocated memory in bytes.
    /// Resolved from either percent × TotalBytes or absolute MB × 1024².
    /// Null = platform default (90% of physical RAM).
    /// <para>Deprecated: new agents should use <see cref="MemoryPercent"/> instead.</para>
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public long? MemoryBytes { get; set; }

    /// <summary>
    /// [Legacy v1] Operator-allocated compute points.
    /// Null = platform default (90% of hardware-max compute points).
    /// <para>Deprecated: new agents should use <see cref="CpuPercent"/> instead.</para>
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? ComputePoints { get; set; }

    /// <summary>
    /// [Legacy v1] Operator-allocated storage in bytes (pre-overcommit).
    /// Null = platform default (90% of physical storage).
    /// <para>Deprecated: new agents should use <see cref="StoragePercent"/> instead.</para>
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public long? StorageBytes { get; set; }

    // =========================================================================
    // Shared fields (both schemas)
    // =========================================================================

    /// <summary>
    /// Operator-allocated GPU count. Discrete resource — absolute, not percent.
    /// Null = all detected GPUs.
    /// 0 = node has GPUs but operator does not offer them.
    /// </summary>
    public int? GpuCount { get; set; }

    /// <summary>
    /// Percentage of proxy-eligible GPU VRAM to offer (0.01–0.95).
    /// Null = all proxy-eligible VRAM (100% default).
    /// </summary>
    public double? GpuVramPercent { get; set; }

    // =========================================================================
    // Constants
    // =========================================================================

    /// <summary>
    /// Platform-wide default allocation percentage for continuous resources.
    /// Applied when operator has not configured a specific limit.
    /// </summary>
    public const double DefaultPercent = 0.90;

    /// <summary>Minimum valid allocation percentage (1%).</summary>
    public const double MinPercent = 0.01;

    /// <summary>Maximum valid allocation percentage (95%).</summary>
    public const double MaxPercent = 0.95;

    /// <summary>Current schema version written by updated agents.</summary>
    public const int CurrentSchemaVersion = 2;

    // =========================================================================
    // Schema detection and migration
    // =========================================================================

    /// <summary>
    /// True when the object uses the legacy absolute-value format.
    /// The orchestrator must convert to percentages before storing.
    /// </summary>
    [JsonIgnore]
    public bool IsLegacyFormat => SchemaVersion < 2;

    /// <summary>
    /// Convert a legacy v1 <see cref="AllocatedResources"/> (absolute values) to
    /// v2 (percentages) using the node's physical hardware totals as the denominator.
    /// Returns a new v2 instance; the original is not modified.
    ///
    /// <para>Any null legacy field maps to a null percentage (platform default applies).</para>
    /// </summary>
    /// <param name="physicalMemoryBytes">Total physical RAM in bytes.</param>
    /// <param name="hardwareMaxComputePoints">
    ///   Hardware-max compute points (cores × pointsPerCore × burstable overcommit).
    /// </param>
    /// <param name="physicalStorageBytes">Total physical storage in bytes.</param>
    /// <returns>A new <see cref="AllocatedResources"/> with <see cref="SchemaVersion"/> = 2.</returns>
    public AllocatedResources ToPercentFormat(
        long physicalMemoryBytes,
        int hardwareMaxComputePoints,
        long physicalStorageBytes)
    {
        double? cpuPct = null;
        if (ComputePoints.HasValue && hardwareMaxComputePoints > 0)
        {
            cpuPct = Clamp((double)ComputePoints.Value / hardwareMaxComputePoints);
        }

        double? memPct = null;
        if (MemoryBytes.HasValue && physicalMemoryBytes > 0)
        {
            memPct = Clamp((double)MemoryBytes.Value / physicalMemoryBytes);
        }

        double? storPct = null;
        if (StorageBytes.HasValue && physicalStorageBytes > 0)
        {
            storPct = Clamp((double)StorageBytes.Value / physicalStorageBytes);
        }

        return new AllocatedResources
        {
            SchemaVersion = CurrentSchemaVersion,
            CpuPercent = cpuPct,
            MemoryPercent = memPct,
            StoragePercent = storPct,
            GpuCount = GpuCount,
            // Clear legacy fields
            MemoryBytes = null,
            ComputePoints = null,
            StorageBytes = null
        };
    }

    /// <summary>
    /// Validate that all percentage fields are within the allowed range.
    /// Returns null if valid, or an error message describing the first violation.
    /// </summary>
    public string? Validate()
    {
        if (SchemaVersion >= 2)
        {
            if (CpuPercent.HasValue && (CpuPercent.Value < MinPercent || CpuPercent.Value > MaxPercent))
                return $"CpuPercent {CpuPercent.Value:P0} outside allowed range ({MinPercent:P0}–{MaxPercent:P0})";

            if (MemoryPercent.HasValue && (MemoryPercent.Value < MinPercent || MemoryPercent.Value > MaxPercent))
                return $"MemoryPercent {MemoryPercent.Value:P0} outside allowed range ({MinPercent:P0}–{MaxPercent:P0})";

            if (StoragePercent.HasValue && (StoragePercent.Value < MinPercent || StoragePercent.Value > MaxPercent))
                return $"StoragePercent {StoragePercent.Value:P0} outside allowed range ({MinPercent:P0}–{MaxPercent:P0})";
        }

        if (GpuCount.HasValue && GpuCount.Value < 0)
            return $"GpuCount cannot be negative ({GpuCount.Value})";

        return null;
    }

    /// <summary>
    /// Resolve the effective CPU allocation percentage, falling back to
    /// <see cref="DefaultPercent"/> when not configured.
    /// </summary>
    [JsonIgnore]
    public double EffectiveCpuPercent => CpuPercent ?? DefaultPercent;

    /// <summary>
    /// Resolve the effective memory allocation percentage, falling back to
    /// <see cref="DefaultPercent"/> when not configured.
    /// </summary>
    [JsonIgnore]
    public double EffectiveMemoryPercent => MemoryPercent ?? DefaultPercent;

    /// <summary>
    /// Resolve the effective storage allocation percentage, falling back to
    /// <see cref="DefaultPercent"/> when not configured.
    /// </summary>
    [JsonIgnore]
    public double EffectiveStoragePercent => StoragePercent ?? DefaultPercent;

    // =========================================================================
    // Internal helpers
    // =========================================================================

    /// <summary>Clamp a ratio to [MinPercent, MaxPercent].</summary>
    private static double Clamp(double ratio)
        => Math.Max(MinPercent, Math.Min(MaxPercent, ratio));
}
