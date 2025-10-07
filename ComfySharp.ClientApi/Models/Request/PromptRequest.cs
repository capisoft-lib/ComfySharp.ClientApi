using System.Text.Json.Serialization;

namespace ComfySharp.ClientApi.Models.Request;

/// <summary>
/// Request model for queuing a ComfyUI workflow for execution
/// </summary>
public class PromptRequest
{
    /// <summary>
    /// The workflow prompt definition as a JSON object (not a string)
    /// </summary>
    [JsonPropertyName("prompt")]
    public System.Text.Json.JsonElement Prompt { get; set; }

    /// <summary>
    /// Optional client identifier for tracking
    /// </summary>
    [JsonPropertyName("client_id")]
    public string? ClientId { get; set; }

    /// <summary>
    /// Additional data for the prompt
    /// </summary>
    [JsonPropertyName("extra_data")]
    public Dictionary<string, object>? ExtraData { get; set; }
}
