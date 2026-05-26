// DeCloud.Shared/Enums/NodeCommandType.cs
namespace DeCloud.Shared.Enums;

/// <summary>
/// Command types dispatched from the orchestrator to a node agent.
/// Integer values are authoritative — they are serialised over the wire
/// and must never be reordered. Add new values at the end only.
///
/// Values 100+ are node-agent-local operations that are never dispatched
/// by the orchestrator directly (they are synthesised on the node side).
/// </summary>
public enum NodeCommandType
{
    CreateVm = 0,
    StopVm = 1,
    StartVm = 2,
    DeleteVm = 3,
    MigrateVm = 4,
    UpdateAgent = 5,
    CollectDiagnostics = 6,
    AllocatePort = 7,
    RemovePort = 8,
    ReseedVm = 9,

    // Node-agent-local — never sent by orchestrator
    UpdateNetwork = 100,
    Benchmark = 101,
    Shutdown = 102,
}