// DeCloud.Shared/Contracts/ApiResponse.cs
namespace DeCloud.Shared.Contracts;

/// <summary>
/// Standard API response envelope used by every orchestrator endpoint.
/// Moved to Shared so the node agent can deserialise typed responses
/// without referencing Orchestrator.Models.
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public ApiError? Error { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }

    public static ApiResponse<T> Ok(T data, Dictionary<string, object>? metadata = null) => new()
    {
        Success = true,
        Data = data,
        Metadata = metadata
    };

    public static ApiResponse<T> Fail(string code, string message, Dictionary<string, object>? details = null) => new()
    {
        Success = false,
        Error = new ApiError(code, message, details)
    };
}

public record ApiError(
    string Code,
    string Message,
    Dictionary<string, object>? Details = null
);