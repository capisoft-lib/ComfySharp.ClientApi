using System.Text.Json.Serialization;

namespace ComfySharp.ClientApi.Models.Response;

/// <summary>
/// Response model for execution history
/// </summary>
public class HistoryResponse
{
    /// <summary>
    /// The prompt ID
    /// </summary>
    [JsonPropertyName("prompt_id")]
    public string PromptId { get; set; } = string.Empty;

    /// <summary>
    /// Execution status information
    /// </summary>
    [JsonPropertyName("status")]
    public PromptStatus Status { get; set; } = new();

    /// <summary>
    /// Output files and results
    /// </summary>
    [JsonPropertyName("outputs")]
    public Dictionary<string, object>? Outputs { get; set; }
}

/// <summary>
/// Status information for a prompt execution
/// </summary>
public class PromptStatus
{
    /// <summary>
    /// Status string (success, error, running, pending)
    /// </summary>
    [JsonPropertyName("status_str")]
    public string StatusStr { get; set; } = string.Empty;

    /// <summary>
    /// Whether the execution is completed
    /// </summary>
    [JsonPropertyName("completed")]
    public bool Completed { get; set; }

    /// <summary>
    /// Status messages
    /// </summary>
    [JsonPropertyName("messages")]
    public List<string>? Messages { get; set; }
}
