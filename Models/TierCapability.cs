using DeCloud.Shared.Enums;

namespace DeCloud.Shared.Models
{
    /// <summary>
    /// Capability information for a specific tier
    /// </summary>
    public class TierCapability
    {
        public QualityTier Tier { get; set; }
        public int MinimumBenchmark { get; set; }
        public double RequiredPointsPerVCpu { get; set; }
        public double NodePointsPerCore { get; set; }
        public int MaxVCpus { get; set; }
        public decimal PriceMultiplier { get; set; }
        public string Description { get; set; } = string.Empty;
        public bool IsEligible { get; set; }
        public string? IneligibilityReason { get; set; }
    }
}
