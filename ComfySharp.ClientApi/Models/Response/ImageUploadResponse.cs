using System.Text.Json.Serialization;

namespace ComfySharp.ClientApi.Models.Response;

/// <summary>
/// Response model for image upload operation
/// </summary>
public class ImageUploadResponse
{
    /// <summary>
    /// Name of the uploaded file
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Subfolder where file was saved
    /// </summary>
    [JsonPropertyName("subfolder")]
    public string Subfolder { get; set; } = string.Empty;

    /// <summary>
    /// File type
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
}
