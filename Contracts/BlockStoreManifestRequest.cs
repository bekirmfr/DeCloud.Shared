// DeCloud.Shared/Contracts/BlockStoreManifestRequest.cs
namespace DeCloud.Shared.Contracts;

/// <summary>
/// Manifest type determines block size and billing rate.
/// Block size is a protocol constant per type — enforced at write time
/// by the block store binary.
/// </summary>
public enum ManifestType
{
    /// <summary>
    /// VM overlay disk replication (lazysync).
    /// Block size: 1 MB. Optimised for sparse write patterns.
    /// </summary>
    VmOverlay = 0,

    /// <summary>
    /// Large language model weight shard for distributed inference.
    /// Block size: 64 MB. Aligned to transformer layer boundaries.
    /// Llama-3 70B Q4 ≈ 640 blocks, FP16 ≈ 2,240 blocks.
    /// </summary>
    ModelShard = 1,

    /// <summary>
    /// LoRA fine-tune adapter weights.
    /// Block size: 256 KB. Fine-grained deduplication across adapter variants.
    /// </summary>
    LoraAdapter = 2,

    /// <summary>
    /// Base OS image template (e.g., debian-12-generic).
    /// Block size: 4 MB. Clean chunk counts, good cross-image deduplication.
    /// </summary>
    ImageTemplate = 3,
}

/// <summary>
/// Wire body for POST /api/blockstore/manifest.
/// Sent by the node agent's LazysyncDaemon; received by BlockStoreController.
/// </summary>
public class BlockStoreManifestRequest
{
    public string VmId { get; set; } = string.Empty;
    public string NodeId { get; set; } = string.Empty;
    public string RootCid { get; set; } = string.Empty;
    public int Version { get; set; }
    public List<string>? ChangedBlockCids { get; set; }
    public Dictionary<long, string>? ChunkMap { get; set; }
    public int BlockCount { get; set; }
    public int BlockSizeKb { get; set; }
    public ManifestType ManifestType { get; set; } = ManifestType.VmOverlay;
    public long TotalBytes { get; set; }
    public bool IsSeeding { get; set; }
    public int ReplicationFactor { get; set; } = 3;
}