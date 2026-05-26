// DeCloud.Shared/Contracts/TerminalContracts.cs
namespace DeCloud.Shared.Contracts;

/// <summary>
/// Request body for POST /api/vms/{vmId}/terminal/connect.
/// Sent by the orchestrator TerminalService; received by node agent SshProxyController.
/// </summary>
public class TerminalConnectRequest
{
    public string VmIp { get; init; } = string.Empty;
    public string? Username { get; init; }
    public int Port { get; init; } = 22;
    public int TtlSeconds { get; init; } = 300;
    public string? Password { get; init; }
}

/// <summary>
/// Response from POST /api/vms/{vmId}/terminal/connect.
/// Sent by node agent SshProxyController; received by orchestrator TerminalService.
/// </summary>
public class TerminalConnectResponse
{
    public bool Success { get; init; }
    public string? Error { get; init; }
    public string WebSocketPath { get; init; } = string.Empty;
    public string PrivateKey { get; init; } = string.Empty;
    public string PrivateKeyBase64 { get; init; } = string.Empty;
    public string Fingerprint { get; init; } = string.Empty;
    public DateTime? ExpiresAt { get; init; }
    public string? MethodUsed { get; init; }
    public string? Password { get; init; }
}