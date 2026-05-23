// src/Shared/AllocationConfig.cs
using System.Text.Json.Serialization;

namespace DeCloud.Shared.Models;

/// <summary>
/// Operator-configured allocation percentages. Stored on the node document
/// as the raw configuration; the orchestrator resolves these into concrete
/// <see cref="ResourceSnapshot"/> values at allocate time.
/// </summary>
public class AllocationConfig
{
    public const double DefaultPercent = 0.90;
    public const double MinPercent = 0.01;
    public const double MaxPercent = 0.95;

    /// <summary>Percentage of physical compute points to offer (0.01–0.95).</summary>
    public double? CpuPercent { get; set; }

    /// <summary>Percentage of physical RAM to offer (0.01–0.95).</summary>
    public double? MemoryPercent { get; set; }

    /// <summary>Percentage of physical storage to offer (0.01–0.95).</summary>
    public double? StoragePercent { get; set; }

    /// <summary>Number of GPUs to offer. Null = all detected.</summary>
    public int? GpuCount { get; set; }

    [JsonIgnore]
    public double EffectiveCpuPercent => CpuPercent ?? DefaultPercent;

    [JsonIgnore]
    public double EffectiveMemoryPercent => MemoryPercent ?? DefaultPercent;

    [JsonIgnore]
    public double EffectiveStoragePercent => StoragePercent ?? DefaultPercent;

    public string? Validate()
    {
        if (CpuPercent.HasValue && (CpuPercent.Value < MinPercent || CpuPercent.Value > MaxPercent))
            return $"CpuPercent {CpuPercent.Value:P0} outside allowed range ({MinPercent:P0}–{MaxPercent:P0})";
        if (MemoryPercent.HasValue && (MemoryPercent.Value < MinPercent || MemoryPercent.Value > MaxPercent))
            return $"MemoryPercent {MemoryPercent.Value:P0} outside allowed range ({MinPercent:P0}–{MaxPercent:P0})";
        if (StoragePercent.HasValue && (StoragePercent.Value < MinPercent || StoragePercent.Value > MaxPercent))
            return $"StoragePercent {StoragePercent.Value:P0} outside allowed range ({MinPercent:P0}–{MaxPercent:P0})";
        if (GpuCount.HasValue && GpuCount.Value < 0)
            return $"GpuCount cannot be negative ({GpuCount.Value})";
        return null;
    }
}