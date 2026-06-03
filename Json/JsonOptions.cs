using System.Text.Json;
using System.Text.Json.Serialization;

namespace DeCloud.Shared.Json;

/// <summary>
/// Platform-wide JSON serialization options shared by the node agent and
/// orchestrator. Use these instead of creating per-call options — ensures
/// the wire format is symmetrical across the boundary by construction,
/// not by hope.
///
/// Two profiles:
///   Default — human-readable (files on disk, diagnostics, metadata.json)
///   Wire    — compact (API responses, heartbeat payloads, state persistence)
///
/// Enum encoding is per-type: decorate the enum with
/// [JsonConverter(typeof(JsonStringEnumConverter))] when it needs to be
/// serialised as a string (the established pattern — see VmCategory,
/// CheckType). Do not add a global enum converter here; it would silently
/// change the wire format of every enum, including those that callers
/// expect as integers (NodeCommandType, QualityTier).
/// </summary>
public static class JsonOptions
{
    /// <summary>
    /// Human-readable: indented, camelCase, case-insensitive, skip nulls.
    /// Use for files written to disk (metadata.json, diagnostics exports).
    /// </summary>
    public static readonly JsonSerializerOptions Default = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = true
    };

    /// <summary>
    /// Compact: same semantics as Default but no indentation.
    /// Use for API payloads, state files, wire format.
    /// </summary>
    public static readonly JsonSerializerOptions Wire = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false
    };
}