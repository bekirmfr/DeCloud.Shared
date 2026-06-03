using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DeCloud.Shared.Enums
{
    /// <summary>
    /// Allowlist of diagnostic streams. Mirrors the node-side enum in
    /// <c>DeCloud.NodeAgent.Infrastructure.Services.DiagnosticSource</c>.
    /// Duplicated rather than shared because the type is small and the only
    /// cross-process contract is the JSON wire format. If a third consumer
    /// appears (e.g. CLI), extract to <c>DeCloud.Shared</c>.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
        public enum DiagnosticSource
    {
        /// <summary>
        /// Host-side <c>console.log</c> written by libvirt's <c>&lt;log&gt;</c>
        /// on the serial chardev. Always readable — no guest cooperation.
        /// </summary>
        Console = 0,

        /// <summary>
        /// Guest <c>/var/log/cloud-init.log</c>. Requires guest agent. Not yet
        /// implemented — reserved for the cloud-init log follow-up.
        /// </summary>
        CloudInitLog = 1,

        /// <summary>
        /// Guest <c>/var/log/cloud-init-output.log</c>. Requires guest agent.
        /// Not yet implemented.
        /// </summary>
        CloudInitOutputLog = 2,

        /// <summary>
        /// Guest <c>journalctl</c> output. Requires guest agent. Not yet
        /// implemented; today this path lives in
        /// <c>SystemVmService.CaptureVmJournalAsync</c>.
        /// </summary>
        Journal = 3
    }
}
