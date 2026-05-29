using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DeCloud.Shared.Enums
{
    /// <summary>
    /// How a service readiness check is performed via qemu-guest-agent
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum CheckType
    {
        /// <summary>
        /// Implicit "System" check: cloud-init status == done.
        /// Always the first service checked; all others gate on this.
        /// </summary>
        CloudInitDone,

        /// <summary>
        /// TCP connect: nc -zv -w2 localhost {port}
        /// </summary>
        TcpPort,

        /// <summary>
        /// HTTP GET: curl -sf -o /dev/null localhost:{port}{path}
        /// </summary>
        HttpGet,

        /// <summary>
        /// Arbitrary command: execute inside VM, exit code 0 = ready
        /// </summary>
        ExecCommand,

        /// <summary>
        /// Host-side guest-agent ping via virsh qemu-agent-command guest-ping.
        /// Does not run through guest-exec — it IS the agent liveness test.
        /// Evaluated before the guest-agent gate in VmReadinessMonitor.
        /// </summary>
        GuestAgentPing
    }
}
