using DeCloud.Shared.Enums;
using System.Text.Json.Serialization;

namespace DeCloud.Shared.Models
{
    /// <summary>
    /// Tracks the readiness status of a single service inside a VM.
    /// The "System" service (cloud-init completion) is always present.
    /// Additional services come from template ExposedPorts.
    /// Checked via qemu-guest-agent from the node agent (hypervisor channel).
    /// </summary>
    public class VmServiceModel
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
        /// How this service is checked for readiness
        /// </summary>
        public CheckType CheckType { get; set; } = CheckType.CloudInitDone;

        /// <summary>
        /// For HttpGet: URL path to check
        /// </summary>
        public string? HttpPath { get; set; }

        /// <summary>
        /// For ExecCommand: command to run inside VM
        /// </summary>
        public string? ExecCommand { get; set; }

        /// <summary>
        /// Current readiness status
        /// </summary>
        public ServiceStatus Status { get; set; } = ServiceStatus.Pending;
        /// <summary>
        /// When true, this service is periodically re-verified after reaching
        /// Ready. A failed re-check reverts Ready → Failed. Default: false.
        /// </summary>
        public bool LivenessCheck { get; set; } = false;

        /// <summary>
        /// Human-readable explanation of current status.
        /// Set on TimedOut/Failed (e.g., "cloud-init error: apt-get install failed"),
        /// cleared when service recovers to Ready.
        /// </summary>
        public string? StatusMessage { get; set; }

        /// <summary>
        /// Last successful HTTP response body from this service's health endpoint
        /// (truncated to 512 chars). Persists across status transitions so the
        /// pre-crash state (memory pressure, OOM count, peer info) survives in
        /// ServicesJson when the VM is deleted. Only populated for HttpGet checks.
        /// </summary>
        public string? LastSuccessBody { get; set; }

        /// <summary>
        /// When the service was first detected as ready
        /// </summary>
        public DateTime? ReadyAt { get; set; }

        /// <summary>
        /// When the last check was performed
        /// </summary>
        public DateTime? LastCheckAt { get; set; }

        /// <summary>
        /// Max seconds to wait before marking TimedOut
        /// </summary>
        public int TimeoutSeconds { get; set; } = 300;

        /// <summary>
        /// In-memory consecutive failed-probe counter for GuestAgentPing services.
        /// Reset to 0 on every successful probe. Used by VmReadinessMonitor to
        /// require sustained failure before transitioning Ready → Failed, matching
        /// the Kubernetes failureThreshold pattern.
        ///
        /// <see cref="JsonIgnoreAttribute"/> is intentional: the counter must reset
        /// on agent restart so the post-restart virtio-channel reconnect window
        /// starts from zero, and the orchestrator never needs to see this
        /// node-internal diagnostic value (it would also be stale by the time it
        /// arrived via heartbeat).
        /// </summary>
        [JsonIgnore]
        public int ConsecutiveFailures { get; set; }
    }
}