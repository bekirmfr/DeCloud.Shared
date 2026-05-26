// DeCloud.Shared/Models/CgnatNodeInfo.cs
using DeCloud.Shared.Enums;

namespace DeCloud.Shared.Models;



/// <summary>
/// Configuration for nodes operating behind CGNAT.
/// Exchanged in both the heartbeat body (node → orchestrator) and the
/// heartbeat response (orchestrator → node). Placed in Shared so both
/// projects reference the same type.
/// </summary>
public class CgnatNodeInfo
{
    /// <summary>ID of the relay node serving this CGNAT node.</summary>
    public string? AssignedRelayNodeId { get; set; }

    /// <summary>WireGuard tunnel IP assigned to this node (e.g. "10.20.3.12").</summary>
    public string TunnelIp { get; set; } = string.Empty;

    /// <summary>Full wg-quick WireGuard config block for connecting to the relay.</summary>
    public string? WireGuardConfig { get; set; }

    /// <summary>
    /// Public endpoint URL for accessing VMs on this node via the relay.
    /// Format: https://relay-{region}-{id}.decloud.io
    /// </summary>
    public string PublicEndpoint { get; set; } = string.Empty;

    /// <summary>Current tunnel connection state.</summary>
    public TunnelStatus TunnelStatus { get; set; } = TunnelStatus.Disconnected;

    /// <summary>Last successful WireGuard handshake with the relay.</summary>
    public DateTime? LastHandshake { get; set; }
}