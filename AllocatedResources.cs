// src/Shared/AllocatedResources.cs
// Operator-configured resource allocation limits.
// Shared between Orchestrator and NodeAgent to ensure wire-compatible serialization.

namespace DeCloud.Shared;

/// <summary>
/// Operator-configured resource allocation limits, resolved to absolute values
/// by the node agent before registration.
///
/// Each field is nullable: null means "use platform default" (90% for continuous
/// resources, all detected for GPU). The orchestrator applies the default when
/// it receives a null field — this preserves backward compatibility with
/// pre-feature agents that do not send this object at all.
///
/// See docs/RESOURCE-ALLOCATION.md for the full design.
/// </summary>
public class AllocatedResources
{
    /// <summary>
    /// Operator-allocated memory in bytes.
    /// Resolved from either percent × TotalBytes or absolute MB × 1024².
    /// Null = platform default (90% of physical RAM).
    /// </summary>
    public long? MemoryBytes { get; set; }

    /// <summary>
    /// Operator-allocated compute points.
    /// Resolved from either percent × TotalComputePoints or absolute points.
    /// Null = platform default (90% of hardware-max compute points).
    /// </summary>
    public int? ComputePoints { get; set; }

    /// <summary>
    /// Operator-allocated storage in bytes (pre-overcommit).
    /// The orchestrator applies tier-specific storage overcommit on top.
    /// Null = platform default (90% of physical storage).
    /// </summary>
    public long? StorageBytes { get; set; }

    /// <summary>
    /// Operator-allocated GPU count.
    /// Null = all detected GPUs.
    /// 0 = node has GPUs but operator does not offer them.
    /// </summary>
    public int? GpuCount { get; set; }

    /// <summary>
    /// Platform-wide default allocation percentage for continuous resources.
    /// Applied when operator has not configured a specific limit.
    /// </summary>
    public const double DefaultPercent = 0.90;
}
