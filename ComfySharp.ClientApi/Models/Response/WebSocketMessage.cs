using System.Text.Json.Serialization;

namespace ComfySharp.ClientApi.Models.Response;

/// <summary>
/// WebSocket message types for real-time communication
/// </summary>
public class WebSocketMessage
{
    /// <summary>
    /// Message type
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Message data
    /// </summary>
    [JsonPropertyName("data")]
    public object? Data { get; set; }

    /// <summary>
    /// Prompt ID if applicable
    /// </summary>
    [JsonPropertyName("prompt_id")]
    public string? PromptId { get; set; }
}

/// <summary>
/// Progress update message from WebSocket
/// </summary>
public class ProgressMessage
{
    /// <summary>
    /// Current step number
    /// </summary>
    [JsonPropertyName("step")]
    public int Step { get; set; }

    /// <summary>
    /// Total number of steps
    /// </summary>
    [JsonPropertyName("total_steps")]
    public int TotalSteps { get; set; }

    /// <summary>
    /// Current step description
    /// </summary>
    [JsonPropertyName("step_description")]
    public string? StepDescription { get; set; }

    /// <summary>
    /// Current node being executed
    /// </summary>
    [JsonPropertyName("current_node")]
    public string? CurrentNode { get; set; }
}

/// <summary>
/// Execution status message from WebSocket
/// </summary>
public class StatusMessage
{
    /// <summary>
    /// Execution status
    /// </summary>
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Status messages
    /// </summary>
    [JsonPropertyName("messages")]
    public List<string>? Messages { get; set; }

    /// <summary>
    /// Output files
    /// </summary>
    [JsonPropertyName("outputs")]
    public Dictionary<string, object>? Outputs { get; set; }
}
