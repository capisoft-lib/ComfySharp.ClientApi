using System.Text.Json.Serialization;

namespace ComfySharp.ClientApi.Models.Response;

/// <summary>
/// Response model for node information
/// </summary>
public class ObjectInfoResponse : Dictionary<string, NodeInfo>
{
}

/// <summary>
/// Information about a specific node type
/// </summary>
public class NodeInfo
{
    /// <summary>
    /// Input parameters for the node
    /// </summary>
    [JsonPropertyName("input")]
    public Dictionary<string, List<string>>? Input { get; set; }

    /// <summary>
    /// Output types for the node
    /// </summary>
    [JsonPropertyName("output")]
    public List<string>? Output { get; set; }

    /// <summary>
    /// Output names for the node
    /// </summary>
    [JsonPropertyName("output_name")]
    public List<string>? OutputName { get; set; }

    /// <summary>
    /// Display name of the node
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of the node
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Category of the node
    /// </summary>
    [JsonPropertyName("category")]
    public string? Category { get; set; }

    /// <summary>
    /// Whether this is an output node
    /// </summary>
    [JsonPropertyName("output_node")]
    public bool OutputNode { get; set; }
}
