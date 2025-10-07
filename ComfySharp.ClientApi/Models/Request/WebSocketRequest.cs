using System.Text.Json.Serialization;

namespace ComfySharp.ClientApi.Models.Request;

/// <summary>
/// Request model for WebSocket connection parameters
/// </summary>
public class WebSocketRequest
{
    /// <summary>
    /// Optional client identifier for the WebSocket connection
    /// </summary>
    [JsonPropertyName("client_id")]
    public string? ClientId { get; set; }
}
