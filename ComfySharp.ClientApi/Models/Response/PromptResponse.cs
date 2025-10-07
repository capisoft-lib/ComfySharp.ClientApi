using System.Text.Json.Serialization;

namespace ComfySharp.ClientApi.Models.Response;

/// <summary>
/// Response model for prompt queue operation
/// </summary>
public class PromptResponse
{
    /// <summary>
    /// Unique identifier for the queued prompt
    /// </summary>
    [JsonPropertyName("prompt_id")]
    public string PromptId { get; set; } = string.Empty;

    /// <summary>
    /// Queue position number
    /// </summary>
    [JsonPropertyName("number")]
    public int Number { get; set; }

    /// <summary>
    /// Any node-specific errors
    /// </summary>
    [JsonPropertyName("node_errors")]
    public Dictionary<string, object>? NodeErrors { get; set; }
}
