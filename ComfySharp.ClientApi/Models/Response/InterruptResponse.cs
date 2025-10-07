using System.Text.Json.Serialization;

namespace ComfySharp.ClientApi.Models.Response;

/// <summary>
/// Response model for interrupt operation
/// </summary>
public class InterruptResponse
{
    /// <summary>
    /// Whether the interrupt signal was sent successfully
    /// </summary>
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    /// <summary>
    /// Response message
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
}
