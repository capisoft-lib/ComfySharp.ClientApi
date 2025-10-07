using System.Text;
using System.Text.Json;
using ComfySharp.ClientApi.Models.Request;
using ComfySharp.ClientApi.Models.Response;

namespace ComfySharp.ClientApi;

/// <summary>
/// Service for interacting with ComfyUI API endpoints
/// </summary>
public class ComfyUIService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    /// <summary>
    /// Initializes a new instance of the ComfyUIService
    /// </summary>
    /// <param name="baseUrl">Base URL of the ComfyUI server</param>
    /// <param name="httpClient">Optional HttpClient instance</param>
    public ComfyUIService(string baseUrl, HttpClient? httpClient = null)
    {
        _httpClient = httpClient ?? new HttpClient();
        _httpClient.BaseAddress = new Uri(baseUrl.TrimEnd('/') + '/');
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            WriteIndented = false
        };
    }

    /// <summary>
    /// Queues a ComfyUI workflow for execution
    /// </summary>
    /// <param name="request">The prompt request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Response containing prompt ID and queue information</returns>
    public async Task<PromptResponse> QueuePromptAsync(PromptRequest request, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(request, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("prompt", content, cancellationToken);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<PromptResponse>(responseContent, _jsonOptions) 
               ?? throw new InvalidOperationException("Failed to deserialize prompt response");
    }

    /// <summary>
    /// Gets the execution history and results for a specific prompt
    /// </summary>
    /// <param name="promptId">The prompt ID to retrieve history for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Execution history and results</returns>
    public async Task<HistoryResponse> GetHistoryAsync(string promptId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"history/{promptId}", cancellationToken);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<HistoryResponse>(responseContent, _jsonOptions) 
               ?? throw new InvalidOperationException("Failed to deserialize history response");
    }

    /// <summary>
    /// Uploads an image to the ComfyUI server
    /// </summary>
    /// <param name="imageData">Image file data</param>
    /// <param name="filename">Optional filename</param>
    /// <param name="overwrite">Whether to overwrite existing file</param>
    /// <param name="subfolder">Subfolder to save the image in</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Upload response with file information</returns>
    public async Task<ImageUploadResponse> UploadImageAsync(
        byte[] imageData, 
        string? filename = null, 
        bool overwrite = false, 
        string subfolder = "input", 
        CancellationToken cancellationToken = default)
    {
        using var formData = new MultipartFormDataContent();
        
        var imageContent = new ByteArrayContent(imageData);
        imageContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
        
        formData.Add(imageContent, "image", filename ?? $"image_{DateTime.UtcNow:yyyyMMdd_HHmmss}.png");
        formData.Add(new StringContent(overwrite.ToString().ToLower()), "overwrite");
        formData.Add(new StringContent(subfolder), "subfolder");

        var response = await _httpClient.PostAsync("upload/image", formData, cancellationToken);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<ImageUploadResponse>(responseContent, _jsonOptions) 
               ?? throw new InvalidOperationException("Failed to deserialize upload response");
    }

    /// <summary>
    /// Retrieves an image file from the ComfyUI server
    /// </summary>
    /// <param name="filename">Name of the image file</param>
    /// <param name="subfolder">Subfolder path</param>
    /// <param name="type">Type of file (input, output, temp)</param>
    /// <param name="format">Image format (png, jpg, webp)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Image file as byte array</returns>
    public async Task<byte[]> GetImageAsync(
        string filename, 
        string? subfolder = null, 
        string type = "output", 
        string format = "png", 
        CancellationToken cancellationToken = default)
    {
        var queryParams = new List<string> { $"filename={Uri.EscapeDataString(filename)}" };
        
        if (!string.IsNullOrEmpty(subfolder))
            queryParams.Add($"subfolder={Uri.EscapeDataString(subfolder)}");
        
        queryParams.Add($"type={Uri.EscapeDataString(type)}");
        queryParams.Add($"format={Uri.EscapeDataString(format)}");

        var url = $"view?{string.Join("&", queryParams)}";
        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsByteArrayAsync(cancellationToken);
    }

    /// <summary>
    /// Gets information about available nodes in ComfyUI
    /// </summary>
    /// <param name="nodeClass">Optional filter by specific node class</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Node information dictionary</returns>
    public async Task<ObjectInfoResponse> GetObjectInfoAsync(string? nodeClass = null, CancellationToken cancellationToken = default)
    {
        var url = "object_info";
        if (!string.IsNullOrEmpty(nodeClass))
        {
            url += $"?node_class={Uri.EscapeDataString(nodeClass)}";
        }

        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<ObjectInfoResponse>(responseContent, _jsonOptions) 
               ?? throw new InvalidOperationException("Failed to deserialize object info response");
    }

    /// <summary>
    /// Gets the current state of the ComfyUI prompt queue
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Queue status with running and pending prompts</returns>
    public async Task<QueueResponse> GetQueueAsync(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync("queue", cancellationToken);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<QueueResponse>(responseContent, _jsonOptions) 
               ?? throw new InvalidOperationException("Failed to deserialize queue response");
    }

    /// <summary>
    /// Interrupts the execution of a currently running prompt
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Interrupt response</returns>
    public async Task<InterruptResponse> InterruptAsync(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsync("interrupt", null, cancellationToken);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<InterruptResponse>(responseContent, _jsonOptions) 
               ?? throw new InvalidOperationException("Failed to deserialize interrupt response");
    }

    /// <summary>
    /// Gets the WebSocket URL for real-time updates
    /// </summary>
    /// <param name="clientId">Optional client identifier</param>
    /// <returns>WebSocket URL</returns>
    public string GetWebSocketUrl(string? clientId = null)
    {
        var baseUrl = _httpClient.BaseAddress?.ToString().TrimEnd('/') ?? "";
        var wsUrl = baseUrl.Replace("http://", "ws://").Replace("https://", "wss://");
        
        if (!string.IsNullOrEmpty(clientId))
        {
            return $"{wsUrl}/ws?client_id={Uri.EscapeDataString(clientId)}";
        }
        
        return $"{wsUrl}/ws";
    }

    /// <summary>
    /// Disposes the HTTP client
    /// </summary>
    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}
