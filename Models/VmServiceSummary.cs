namespace DeCloud.Shared.Models
{
    /// <summary>
    /// Lightweight service status for heartbeat reporting.
    /// </summary>
    public class VmServiceSummary
    {
        public string Name { get; set; } = string.Empty;
        public int? Port { get; set; }
        public string? Protocol { get; set; }
        public string Status { get; set; } = "Pending";
        public string? StatusMessage { get; set; }
        public DateTime? ReadyAt { get; set; }
    }
}
