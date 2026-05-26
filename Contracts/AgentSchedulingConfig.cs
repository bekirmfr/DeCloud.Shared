// DeCloud.Shared/Contracts/AgentSchedulingConfig.cs
using DeCloud.Shared.Enums;

namespace DeCloud.Shared.Contracts;

/// <summary>
/// Per-tier scheduling parameters shared between orchestrator and node agent.
/// </summary>
public class TierConfiguration
{
    public int MinimumBenchmark { get; set; }
    /// <summary>CPU overcommit ratio (e.g. 4.0 for Burstable).</summary>
    public double CpuOvercommitRatio { get; set; }
    public double StorageOvercommitRatio { get; set; }
    public decimal PriceMultiplier { get; set; }
    public string Description { get; set; } = string.Empty;
    public string TargetUseCase { get; set; } = string.Empty;
}

/// <summary>
/// Lightweight scheduling configuration pushed from orchestrator → node agent.
/// Contains only the fields the node agent needs for CPU quota calculations.
/// The orchestrator's full <c>SchedulingConfig</c> (MongoDB document) stays
/// in <c>Orchestrator.Models</c>; this projection lives in Shared.
/// </summary>
public class AgentSchedulingConfig
{
    /// <summary>Monotonic version for change detection.</summary>
    public int Version { get; set; }
    /// <summary>Baseline benchmark score (e.g. 1000 for Intel i3-10100).</summary>
    public int BaselineBenchmark { get; set; } = 1000;
    /// <summary>Burstable tier CPU overcommit ratio.</summary>
    public double BaselineOvercommitRatio { get; set; } = 4.0;
    /// <summary>Maximum performance multiplier cap.</summary>
    public double MaxPerformanceMultiplier { get; set; } = 20.0;
    /// <summary>Per-tier configuration used by node agent for quota enforcement.</summary>
    public Dictionary<QualityTier, TierConfiguration> Tiers { get; set; } = new();
    public DateTime UpdatedAt { get; set; }
}