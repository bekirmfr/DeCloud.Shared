using DeCloud.Shared.Models;

namespace DeCloud.Shared.Contracts
{
    /// <summary>
    /// Node registration with the orchestrator
    /// </summary>
    public class NodeRegistrationRequest
    {
        /// <summary>
        /// Machine fingerprint for validation
        /// </summary>
        public required string MachineId { get; set; }
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Wallet address for ownership/billing
        /// </summary>
        public required string WalletAddress { get; set; }
        public string PublicIp { get; set; } = string.Empty;
        public int AgentPort { get; set; }

        // Full resource inventory
        public required HardwareInventory HardwareInventory { get; set; } = new();
        public required string AgentVersion { get; set; } = string.Empty;
        public required List<string> SupportedImages { get; set; } = new();

        /// <summary>
        /// SSH certificate authority public key — captured from
        /// <c>/etc/ssh/decloud_ca.pub</c> by <c>OrchestratorClient.RegisterAsync</c>
        /// at registration time. Sent to the orchestrator so tenant cloud-init
        /// templates can substitute <c>__CA_PUBLIC_KEY__</c> at render time.
        ///
        /// <para>
        /// Mirrors the orchestrator-side
        /// <c>NodeRegistrationRequest.SshCaPublicKey</c>. Same JSON shape on the
        /// wire.
        /// </para>
        ///
        /// <para>
        /// Null on the rare path where the node lacks <c>/etc/ssh/decloud_ca.pub</c>
        /// (e.g., misconfigured or freshly cloned node image). The orchestrator
        /// accepts null and stamps null into <c>Node.SshCaPublicKey</c>; tenant
        /// deploys that need the CA key fail at render time with a clear message.
        /// </para>
        /// </summary>
        public string? SshCaPublicKey { get; set; }


        // Staking info
        public string StakingTxHash { get; set; } = string.Empty;

        public string Region { get; set; } = "default";
        public string Zone { get; set; } = "default";

        /// <summary>
        /// ISO 3166-1 alpha-2 country code declared by the operator.
        /// Read from <c>Node:Country</c> in appsettings.Production.json.
        /// <c>"ZZ"</c> when not configured. Null on nodes running pre-2.3
        /// agents — orchestrator accepts null and records "ZZ".
        /// </summary>
        public string? Country { get; set; }


        /// <summary>
        /// Wallet signature proving ownership (from WalletConnect CLI)
        /// Optional for backward compatibility - will be required in production
        /// </summary>
        public string? Signature { get; set; }

        /// <summary>
        /// Message that was signed (includes node ID, wallet, timestamp)
        /// Optional for backward compatibility - will be required in production
        /// </summary>
        public string? Message { get; set; }

        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Node operator pricing (optional). If null, platform defaults are used.
        /// </summary>
        public NodePricing? Pricing { get; set; }

        /// <summary>
        /// Monotonic version from the orchestrator — used for conflict resolution.
        /// </summary>
        public Dictionary<string, int> ObligationStateVersions { get; set; } = new();

        /// <summary>
        /// Revisions of system templates currently stored in the node agent's
        /// SQLite database, keyed by canonical role name.
        /// Allows the orchestrator to skip sending templates already current on the node.
        /// Absent or zero-valued entries mean no template stored for that role.
        /// </summary>
        public Dictionary<string, int>? SystemTemplateVersions { get; set; }

        /// <summary>
        /// Operator-configured resource allocation limits, resolved to absolute
        /// values. Null = use platform default (90%).
        /// </summary>
        public DeCloud.Shared.Models.AllocatedResources? AllocatedResources { get; set; }
    }
}
