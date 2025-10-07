using System.Text.Json.Serialization;

namespace ComfySharp.ClientApi.Models.Response;

/// <summary>
/// Response model for queue status
/// </summary>
public class QueueResponse
{
    /// <summary>
    /// Currently running prompts
    /// </summary>
    [JsonPropertyName("queue_running")]
    public List<QueueItem> QueueRunning { get; set; } = new();

    /// <summary>
    /// Pending prompts in queue
    /// </summary>
    [JsonPropertyName("queue_pending")]
    public List<QueueItem> QueuePending { get; set; } = new();
}

/// <summary>
/// Represents an item in the queue
/// </summary>
public class QueueItem
{
    /// <summary>
    /// The prompt ID
    /// </summary>
    [JsonPropertyName("prompt_id")]
    public string PromptId { get; set; } = string.Empty;

    /// <summary>
    /// Status information
    /// </summary>
    [JsonPropertyName("status")]
    public PromptStatus Status { get; set; } = new();

    /// <summary>
    /// Output files and results
    /// </summary>
    [JsonPropertyName("outputs")]
    public Dictionary<string, object>? Outputs { get; set; }
}
