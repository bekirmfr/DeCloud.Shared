// DeCloud.Shared/Dtos/ObligationDescriptorDto.cs
namespace DeCloud.Shared.Dtos;

/// <summary>
/// Wire DTO for a single obligation exchanged between orchestrator and node agent.
/// Carries the canonical role name and its dependency list so the node can
/// populate its local obligation table without contacting the orchestrator again.
/// </summary>
public class ObligationDescriptorDto
{
    public string Role { get; init; } = string.Empty;
    public List<string> Deps { get; init; } = [];
}