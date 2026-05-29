namespace DeCloud.Shared.Contracts;

using DeCloud.Shared.Enums;
using DeCloud.Shared.Models; // TemplateArtifact

/// <summary>Wire contract for NodeCommandType.CreateVm. Single source of truth
/// for the orchestrator→node CreateVm payload. Superset of every field the
/// node's HandleCreateVmAsync consumes. Carries transient secrets (Password,
/// sensitive Labels) that the node must NOT persist beyond VM creation.</summary>
public sealed class CreateVmPayload
{
    // Identity (invariants)
    public required string VmId { get; init; }
    public required string Name { get; init; }
    public required VmCategory Category { get; init; }
    public required VmRole Role { get; init; }

    public string? OwnerId { get; init; }
    public string? OwnerWallet { get; init; }

    // Resources
    public required int VirtualCpuCores { get; init; }
    public required long MemoryBytes { get; init; }
    public required long DiskBytes { get; init; }
    public QualityTier QualityTier { get; init; } = QualityTier.Standard;
    public int ComputePointCost { get; init; }

    // GPU
    public GpuMode GpuMode { get; init; } = GpuMode.None;
    public string? GpuPciAddress { get; init; }
    public long? GpuVramBytes { get; init; }

    // Deployment
    public DeploymentMode DeploymentMode { get; init; } = DeploymentMode.VirtualMachine;
    public string? ContainerImage { get; init; }
    public string? BaseImageUrl { get; init; }
    public string? CloudInitUserData { get; init; }      // FIXES the current name mismatch
    public string? SshPublicKey { get; init; }
    public Dictionary<string, string>? EnvironmentVariables { get; init; }

    // Security (transient — node persists encrypted only; secrets stripped orchestrator-side)
    public string? Password { get; init; }
    public Dictionary<string, string>? Labels { get; init; }

    // Replication / placement / migration
    public int ReplicationFactor { get; init; }
    public string? TargetNodeId { get; init; }
    public string? ManifestRootCid { get; init; }
    public Dictionary<long, string>? ChunkMap { get; init; }

    // Pipeline inputs (reuse existing Shared types — don't invent parallel ones)
    public List<TemplateArtifact> Artifacts { get; init; } = new();
    public List<VmServiceDefinition> Services { get; init; } = new();
}