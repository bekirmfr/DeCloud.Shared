using DeCloud.Shared.Enums;

namespace DeCloud.Shared.Models
{
    public class VmSummary
    {
        public required string VmId { get; set; }
        public required string Name { get; set; }
        public required VmStatus Status { get; set; }
        public required VmCategory Category { get; set; } = VmCategory.Tenant;
        public required VmRole Role { get; set; } = VmRole.General;
        public string? OwnerId { get; set; } = null;
        public int VirtualCpuCores { get; set; }
        public int QualityTier { get; set; }
        public int ComputePointCost { get; set; }
        public long MemoryBytes { get; set; }
        public long DiskBytes { get; set; }
        /// <summary>GPU access mode integer value (GpuMode enum): 0=None, 1=Passthrough, 2=Proxied.</summary>
        public int GpuMode { get; set; }
        /// <summary>VRAM quota for Proxied mode in bytes. Null = no quota.</summary>
        public long GpuVramBytes { get; set; } = 0L;
        public double VirtualCpuUsagePercent { get; set; }
        public bool IsIpAssigned { get; set; }
        public string? IpAddress { get; set; }
        public int? VncPort { get; set; }
        public string? MacAddress { get; set; }
        public List<VmServiceSummary>? Services { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// HTTPS URL the base image was downloaded from. Reported to the
        /// orchestrator so it can detect drift between what it told the node
        /// to use and what the node actually used.
        /// See BASE_IMAGE_DESIGN.md §4.5.
        /// </summary>
        public string? BaseImageUrl { get; set; }

        /// <summary>
        /// SHA256 of the cached base image bytes (lowercase hex, 64 chars).
        /// First-deploy discovery: node computes after download and reports
        /// here; orchestrator adopts on first non-empty heartbeat. Migration:
        /// node reports the strictly-verified hash here.
        /// See BASE_IMAGE_DESIGN.md §4.5.
        /// </summary>
        public string? BaseImageHash { get; set; }
    }
}