using System.Text.Json.Serialization;

namespace ComfySharp.ClientApi.Models.Request;

/// <summary>
/// Request model for retrieving an image from ComfyUI
/// </summary>
public class ViewImageRequest
{
    /// <summary>
    /// Name of the image file
    /// </summary>
    [JsonPropertyName("filename")]
    public string Filename { get; set; } = string.Empty;

    /// <summary>
    /// Subfolder path
    /// </summary>
    [JsonPropertyName("subfolder")]
    public string? Subfolder { get; set; }

    /// <summary>
    /// Type of file (input, output, temp)
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = "output";

    /// <summary>
    /// Image format (png, jpg, webp)
    /// </summary>
    [JsonPropertyName("format")]
    public string Format { get; set; } = "png";
}
