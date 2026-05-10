using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace DeCloud.Shared;

/// <summary>
/// Computes a deterministic SHA-256 hash of node settings for drift
/// detection. Used by the node agent (compute and send in heartbeat)
/// and the orchestrator (compute expected and compare).
///
/// The hash covers the fields that the orchestrator considers
/// authoritative from registration: wallet, country, region, zone.
/// Cosmetic fields (name, description) and transient fields
/// (signature, signed_at) are excluded — they don't affect
/// scheduling or trust.
/// </summary>
public static class SettingsHash
{
    /// <summary>
    /// Compute hash from individual field values.
    /// Both sides call this with the same inputs to get the same output.
    /// </summary>
    public static string Compute(
        string walletAddress,
        string country,
        string region)
    {
        // Canonical form: sorted key-value pairs, lowercase, no whitespace.
        // This is intentionally simple — no JSON serialization, no
        // dependency on field ordering or formatting. Just a stable
        // concatenation that both sides can reproduce.
        var canonical = string.Join('\n',
            $"country:{(country ?? "ZZ").ToUpperInvariant()}",
            $"region:{(region ?? "unknown").ToLowerInvariant()}",
            $"wallet:{(walletAddress ?? "").ToLowerInvariant()}"
        );

        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(canonical));
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
}