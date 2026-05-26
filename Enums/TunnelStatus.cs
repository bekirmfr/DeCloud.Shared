namespace DeCloud.Shared.Enums
{
    /// <summary>
    /// WireGuard tunnel connection status between a CGNAT node and its relay.
    /// </summary>
    public enum TunnelStatus
    {
        Connecting,
        Connected,
        Disconnected,
        Error
    }
}
