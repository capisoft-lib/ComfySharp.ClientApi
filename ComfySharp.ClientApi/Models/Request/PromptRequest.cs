using System.Text.Json.Serialization;

namespace ComfySharp.ClientApi.Models.Request;

/// <summary>
/// Request model for queuing a ComfyUI workflow for execution
/// </summary>
public class PromptRequest
{
    /// <summary>
    /// The workflow prompt definition containing node configurations
    /// </summary>
    [JsonPropertyName("prompt")]
    public Dictionary<string, object> Prompt { get; set; } = new();

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
