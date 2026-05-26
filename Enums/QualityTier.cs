namespace DeCloud.Shared.Enums
{
    /// <summary>
    /// Quality tier for VM scheduling — used in both orchestrator decisions
    /// and node agent CPU quota calculations.
    /// </summary>
    public enum QualityTier
    {
        /// <summary>Dedicated resources, 1:1 CPU. Requires 4000+ benchmark.</summary>
        Guaranteed = 0,
        /// <summary>High performance, 1.6:1 CPU. Requires 2500+ benchmark.</summary>
        Standard = 1,
        /// <summary>Balanced, 2.7:1 CPU. Requires 1500+ benchmark.</summary>
        Balanced = 2,
        /// <summary>Best-effort, 4:1 CPU. Minimum 1000 benchmark.</summary>
        Burstable = 3
    }
}
