// src/Shared/NodeAllocateResponse.cs
// Wire type for the response from POST /api/nodes/{id}/allocate.

namespace DeCloud.Shared.Contracts;

/// <summary>
/// Response from the resource allocation endpoint, confirming
/// the stored allocation percentages and their effective absolute values
/// (computed from the current performance evaluation, if available).
/// </summary>
public class NodeAllocateResponse
{
    /// <summary>Whether the allocation was accepted and stored.</summary>
    public bool Success { get; set; }

    /// <summary>Error message if <see cref="Success"/> is false.</summary>
    public string? Error { get; set; }

    /// <summary>Stored CPU allocation percentage.</summary>
    public double EffectiveCpuPercent { get; set; }

    /// <summary>Stored memory allocation percentage.</summary>
    public double EffectiveMemoryPercent { get; set; }

    /// <summary>Stored storage allocation percentage.</summary>
    public double EffectiveStoragePercent { get; set; }

    /// <summary>Stored GPU count (null = all detected).</summary>
    public int? GpuCount { get; set; }

    /// <summary>
    /// Absolute compute points this percentage maps to, given
    /// the current evaluation. Null if the node hasn't been evaluated yet.
    /// </summary>
    public int? ResolvedComputePoints { get; set; }

    /// <summary>
    /// Absolute memory bytes this percentage maps to, given
    /// physical RAM. Null if hardware inventory unavailable.
    /// </summary>
    public long? ResolvedMemoryBytes { get; set; }

    /// <summary>
    /// Absolute storage bytes this percentage maps to (pre-overcommit),
    /// given physical storage. Null if hardware inventory unavailable.
    /// </summary>
    public long? ResolvedStorageBytes { get; set; }
}