using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeCloud.Shared.Utils
{
    public class ArchitectureHelper
    {
        /// <summary>
        /// Normalise a node's reported CPU architecture string to the tag used
        /// by <c>TemplateArtifact.Architecture</c>. Mirrors the logic in
        /// <c>VmService.TryScheduleVmAsync</c>'s STEP 6.5.
        /// </summary>
        public static string NormaliseArchitecture(string? raw) =>
            raw?.ToLowerInvariant() switch
            {
                "arm64" or "aarch64" => "arm64",
                "x86_64" or "amd64" => "amd64",
                _ => "amd64",
            };
    }
}
