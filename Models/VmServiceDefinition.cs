namespace DeCloud.Shared.Models
{
    /// <summary>
    /// A single service readiness declaration carried in the CreateVm wire payload.
    /// The "System" service (cloud-init completion) is always present.
    /// Additional services come from template ExposedPorts.
    /// Checked via qemu-guest-agent from the node agent (hypervisor channel).
    ///
    /// This is the wire *definition* only — runtime state (Status, ReadyAt, etc.)
    /// lives in the node's VmServiceStatus, which the node builds from this.
    /// </summary>
    public class VmServiceDefinition
    {
        /// <summary>
        /// Service display name (e.g., "System", "PostgreSQL", "VS Code Server")
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Port number for this service. Null for "System" (cloud-init).
        /// </summary>
        public int? Port { get; set; }

        /// <summary>
        /// Protocol (tcp, http, udp, etc.). Null for "System".
        /// </summary>
        public string? Protocol { get; set; }

        /// <summary>
        /// How this service is checked for readiness. Carried as a string to match
        /// the existing SystemVmServiceDeclaration convention and to keep this Shared
        /// contract free of the node-only CheckType enum (which has a GuestAgentPing
        /// member that is never sent over the wire). Valid values mirror the node's
        /// CheckType enum: "CloudInitDone" | "TcpPort" | "HttpGet" | "ExecCommand".
        /// The node parses this with a case-insensitive Enum.TryParse and falls back
        /// to CloudInitDone for unknown/empty values.
        /// </summary>
        public string CheckType { get; set; } = "CloudInitDone";

        /// <summary>
        /// For HttpGet: URL path to check
        /// </summary>
        public string? HttpPath { get; set; }

        /// <summary>
        /// For ExecCommand: command to run inside VM
        /// </summary>
        public string? ExecCommand { get; set; }

        /// <summary>
        /// Max seconds to wait before marking TimedOut
        /// </summary>
        public int TimeoutSeconds { get; set; } = 300;
    }
}