using System.Text.Json.Serialization;

namespace ComfySharp.ClientApi.Models.Request;

/// <summary>
/// Request model for uploading an image to ComfyUI
/// </summary>
public class ImageUploadRequest
{
    /// <summary>
    /// Image file data as byte array
    /// </summary>
    [JsonPropertyName("image")]
    public byte[] Image { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// Whether to overwrite existing file
    /// </summary>
    [JsonPropertyName("overwrite")]
    public bool Overwrite { get; set; } = false;

    /// <summary>
    /// Subfolder to save the image in
    /// </summary>
    [JsonPropertyName("subfolder")]
    public string Subfolder { get; set; } = "input";
}
