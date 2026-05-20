// src/Shared/NodeAllocateRequest.cs
// Wire type for POST /api/nodes/{id}/allocate.
// Shared between NodeAgent (sender) and Orchestrator (receiver).

using DeCloud.Shared.Models;

namespace DeCloud.Shared.Contracts;

/// <summary>
/// Request body for the resource allocation endpoint.
/// The operator sets allocation percentages via <c>decloud allocate</c>;
/// the agent pushes them to the orchestrator via this DTO.
///
/// All percentage fields are optional. A null field means "keep the current
/// value" (or "use platform default" if no value was ever set).
///
/// See docs/NODE-LIFECYCLE.md §3 for the full design.
/// </summary>
public class NodeAllocateRequest
{
    /// <summary>
    /// Percentage of hardware-max compute points to offer (0.01–0.95).
    /// Null = keep current / platform default.
    /// </summary>
    public double? CpuPercent { get; set; }

    /// <summary>
    /// Percentage of physical RAM to offer (0.01–0.95).
    /// Null = keep current / platform default.
    /// </summary>
    public double? MemoryPercent { get; set; }

    /// <summary>
    /// Percentage of physical storage to offer (0.01–0.95).
    /// Null = keep current / platform default.
    /// </summary>
    public double? StoragePercent { get; set; }

    /// <summary>
    /// Number of GPUs to offer. Null = keep current / all detected. 0 = none.
    /// </summary>
    public int? GpuCount { get; set; }

    /// <summary>
    /// Validate all fields are within allowed ranges.
    /// Returns null if valid, or an error message on first violation.
    /// </summary>
    public string? Validate()
    {
        if (CpuPercent.HasValue &&
            (CpuPercent.Value < AllocatedResources.MinPercent ||
             CpuPercent.Value > AllocatedResources.MaxPercent))
            return $"CpuPercent {CpuPercent.Value:P0} outside allowed range " +
                   $"({AllocatedResources.MinPercent:P0}–{AllocatedResources.MaxPercent:P0})";

        if (MemoryPercent.HasValue &&
            (MemoryPercent.Value < AllocatedResources.MinPercent ||
             MemoryPercent.Value > AllocatedResources.MaxPercent))
            return $"MemoryPercent {MemoryPercent.Value:P0} outside allowed range " +
                   $"({AllocatedResources.MinPercent:P0}–{AllocatedResources.MaxPercent:P0})";

        if (StoragePercent.HasValue &&
            (StoragePercent.Value < AllocatedResources.MinPercent ||
             StoragePercent.Value > AllocatedResources.MaxPercent))
            return $"StoragePercent {StoragePercent.Value:P0} outside allowed range " +
                   $"({AllocatedResources.MinPercent:P0}–{AllocatedResources.MaxPercent:P0})";

        if (GpuCount.HasValue && GpuCount.Value < 0)
            return $"GpuCount cannot be negative ({GpuCount.Value})";

        return null;
    }

    /// <summary>
    /// Merge this request into an existing <see cref="AllocatedResources"/>,
    /// applying only the non-null fields. Returns a new v2 instance.
    /// </summary>
    public AllocatedResources ApplyTo(AllocatedResources? existing)
    {
        existing ??= new AllocatedResources { SchemaVersion = AllocatedResources.CurrentSchemaVersion };

        return new AllocatedResources
        {
            SchemaVersion = AllocatedResources.CurrentSchemaVersion,
            CpuPercent = CpuPercent ?? existing.CpuPercent,
            MemoryPercent = MemoryPercent ?? existing.MemoryPercent,
            StoragePercent = StoragePercent ?? existing.StoragePercent,
            GpuCount = GpuCount ?? existing.GpuCount,
            // Legacy fields cleared on v2
            MemoryBytes = null,
            ComputePoints = null,
            StorageBytes = null
        };
    }
}