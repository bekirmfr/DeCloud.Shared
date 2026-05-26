// DeCloud.Shared/Contracts/NodeCommand.cs
using DeCloud.Shared.Enums;

namespace DeCloud.Shared.Contracts;

/// <summary>
/// Command dispatched from the orchestrator to a node agent.
/// This is the wire type — the node receives this and deserialises it.
/// </summary>
public record NodeCommand(
    string CommandId,
    NodeCommandType Type,
    string Payload,
    bool RequiresAck = true,
    string? TargetResourceId = null
)
{
    public DateTime QueuedAt { get; init; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; init; }
    public bool IsExpired => ExpiresAt.HasValue && DateTime.UtcNow > ExpiresAt.Value;
    public TimeSpan Age => DateTime.UtcNow - QueuedAt;
}

/// <summary>
/// Acknowledgment sent from node agent back to orchestrator
/// after a command completes (success or failure).
/// </summary>
public record CommandAcknowledgment(
    string CommandId,
    bool Success,
    string? ErrorMessage,
    DateTime CompletedAt,
    string? Data = null
);