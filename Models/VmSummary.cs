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
        public List<ServiceSummary>? Services { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
