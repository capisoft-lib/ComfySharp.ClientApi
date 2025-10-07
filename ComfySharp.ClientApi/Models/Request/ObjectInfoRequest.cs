using System.Text.Json.Serialization;

namespace ComfySharp.ClientApi.Models.Request;

/// <summary>
/// Request model for retrieving node information
/// </summary>
public class ObjectInfoRequest
{
    /// <summary>
    /// Filter by specific node class
    /// </summary>
    [JsonPropertyName("node_class")]
    public string? NodeClass { get; set; }
}
