using System.Text.Json.Serialization;

namespace DeCloud.Shared.Enums
{
    /// <summary>
    /// Readiness state of a single service inside a VM
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ServiceStatus
    {
        /// <summary>Waiting for System (cloud-init) to complete first</summary>
        Pending,

        /// <summary>Actively being probed</summary>
        Checking,

        /// <summary>Check passed — service is accepting traffic</summary>
        Ready,

        /// <summary>Timeout expired without the check passing</summary>
        TimedOut,

        /// <summary>Cloud-init reported error (System service only)</summary>
        Failed
    }
}