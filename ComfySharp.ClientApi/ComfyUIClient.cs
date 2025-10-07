using ComfySharp.ClientApi.Models.Request;
using ComfySharp.ClientApi.Models.Response;
using ComfySharp.ClientApi.WebSocket;
using ComfySharp.ClientApi.Extensions;
using ComfySharp;

namespace ComfySharp.ClientApi;

/// <summary>
/// Main client for interacting with ComfyUI API
/// </summary>
public class ComfyUIClient : IDisposable
{
    private readonly ComfyUIService _httpService;
    private readonly ComfyUIWebSocketService _webSocketService;
    private readonly string _clientId;
    private bool _disposed = false;

    /// <summary>
    /// Initializes a new instance of the ComfyUIClient
    /// </summary>
    /// <param name="baseUrl">Base URL of the ComfyUI server</param>
    /// <param name="httpClient">Optional HttpClient instance</param>
    /// <param name="clientId">Optional client identifier. If not provided, a unique ID will be generated</param>
    public ComfyUIClient(string baseUrl, HttpClient? httpClient = null, string? clientId = null)
    {
        _clientId = clientId ?? GenerateClientId();
        _httpService = new ComfyUIService(baseUrl, httpClient);
        _webSocketService = new ComfyUIWebSocketService();
    }

    /// <summary>
    /// Gets the client ID used for this instance
    /// </summary>
    public string ClientId => _clientId;

    #region HTTP API Methods

    /// <summary>
    /// Queues a ComfyUI workflow for execution
    /// </summary>
    /// <param name="request">The prompt request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Response containing prompt ID and queue information</returns>
    public async Task<PromptResponse> QueuePromptAsync(PromptRequest request, CancellationToken cancellationToken = default)
    {
        return await _httpService.QueuePromptAsync(request, cancellationToken);
    }

    /// <summary>
    /// Queues a ComfyUI workflow for execution
    /// </summary>
    /// <param name="workflow">The ComfyUI workflow to execute</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Response containing prompt ID and queue information</returns>
    public async Task<PromptResponse> QueuePromptAsync(ComfyUIWorkflow workflow, CancellationToken cancellationToken = default)
    {
        var request = workflow.ToPromptRequest(_clientId);
        return await _httpService.QueuePromptAsync(request, cancellationToken);
    }

    /// <summary>
    /// Gets the execution history and results for a specific prompt
    /// </summary>
    /// <param name="promptId">The prompt ID to retrieve history for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Execution history and results</returns>
    public async Task<HistoryResponse> GetHistoryAsync(string promptId, CancellationToken cancellationToken = default)
    {
        return await _httpService.GetHistoryAsync(promptId, cancellationToken);
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
        return await _httpService.UploadImageAsync(imageData, filename, overwrite, subfolder, cancellationToken);
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
        return await _httpService.GetImageAsync(filename, subfolder, type, format, cancellationToken);
    }

    /// <summary>
    /// Gets information about available nodes in ComfyUI
    /// </summary>
    /// <param name="nodeClass">Optional filter by specific node class</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Node information dictionary</returns>
    public async Task<ObjectInfoResponse> GetObjectInfoAsync(string? nodeClass = null, CancellationToken cancellationToken = default)
    {
        return await _httpService.GetObjectInfoAsync(nodeClass, cancellationToken);
    }

    /// <summary>
    /// Gets the current state of the ComfyUI prompt queue
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Queue status with running and pending prompts</returns>
    public async Task<QueueResponse> GetQueueAsync(CancellationToken cancellationToken = default)
    {
        return await _httpService.GetQueueAsync(cancellationToken);
    }

    /// <summary>
    /// Interrupts the execution of a currently running prompt
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Interrupt response</returns>
    public async Task<InterruptResponse> InterruptAsync(CancellationToken cancellationToken = default)
    {
        return await _httpService.InterruptAsync(cancellationToken);
    }

    #endregion

    #region WebSocket Methods

    /// <summary>
    /// Connects to the ComfyUI WebSocket for real-time updates
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task ConnectWebSocketAsync(CancellationToken cancellationToken = default)
    {
        var wsUrl = _httpService.GetWebSocketUrl(_clientId);
        await _webSocketService.ConnectAsync(wsUrl, cancellationToken);
    }

    /// <summary>
    /// Disconnects from the WebSocket
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task DisconnectWebSocketAsync(CancellationToken cancellationToken = default)
    {
        await _webSocketService.DisconnectAsync(cancellationToken);
    }

    /// <summary>
    /// Gets whether the WebSocket is connected
    /// </summary>
    public bool IsWebSocketConnected => _webSocketService.IsConnected;

    /// <summary>
    /// Subscribe to typed WebSocket messages. Returns IDisposable for unsubscription.
    /// </summary>
    public IDisposable Subscribe<T>(Action<T> handler) where T : class => _webSocketService.Subscribe(handler);

    /// <summary>
    /// Subscribe to raw WebSocket messages.
    /// </summary>
    public IDisposable SubscribeRaw(Action<WebSocketMessage> handler) => _webSocketService.SubscribeRaw(handler);

    /// <summary>
    /// Subscribe to progress messages.
    /// </summary>
    public IDisposable SubscribeProgress(Action<ProgressMessage> handler) => _webSocketService.SubscribeProgress(handler);

    /// <summary>
    /// Subscribe to status messages.
    /// </summary>
    public IDisposable SubscribeStatus(Action<StatusMessage> handler) => _webSocketService.SubscribeStatus(handler);

    #endregion

    #region WebSocket Events

    /// <summary>
    /// Event fired when a progress update is received via WebSocket
    /// </summary>
    public event EventHandler<ProgressMessage>? ProgressReceived
    {
        add => _webSocketService.ProgressReceived += value;
        remove => _webSocketService.ProgressReceived -= value;
    }

    /// <summary>
    /// Event fired when a status update is received via WebSocket
    /// </summary>
    public event EventHandler<StatusMessage>? StatusReceived
    {
        add => _webSocketService.StatusReceived += value;
        remove => _webSocketService.StatusReceived -= value;
    }

    /// <summary>
    /// Event fired when a general message is received via WebSocket
    /// </summary>
    public event EventHandler<WebSocketMessage>? MessageReceived
    {
        add => _webSocketService.MessageReceived += value;
        remove => _webSocketService.MessageReceived -= value;
    }

    /// <summary>
    /// Event fired when the WebSocket connection is established
    /// </summary>
    public event EventHandler? WebSocketConnected
    {
        add => _webSocketService.Connected += value;
        remove => _webSocketService.Connected -= value;
    }

    /// <summary>
    /// Event fired when the WebSocket connection is closed
    /// </summary>
    public event EventHandler? WebSocketDisconnected
    {
        add => _webSocketService.Disconnected += value;
        remove => _webSocketService.Disconnected -= value;
    }

    /// <summary>
    /// Event fired when a WebSocket error occurs
    /// </summary>
    public event EventHandler<Exception>? WebSocketErrorOccurred
    {
        add => _webSocketService.ErrorOccurred += value;
        remove => _webSocketService.ErrorOccurred -= value;
    }

    #endregion

    #region Convenience Methods

    /// <summary>
    /// Queues a workflow and waits for completion with real-time updates
    /// </summary>
    /// <param name="request">The prompt request</param>
    /// <param name="timeout">Maximum time to wait for completion</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Final execution results</returns>
    public async Task<HistoryResponse> QueueAndWaitForCompletionAsync(
        PromptRequest request, 
        TimeSpan timeout = default, 
        CancellationToken cancellationToken = default)
    {
        if (timeout == default)
            timeout = TimeSpan.FromMinutes(10);

        // Queue the prompt
        var promptResponse = await QueuePromptAsync(request, cancellationToken);
        
        // Wait for completion
        var startTime = DateTime.UtcNow;
        while (DateTime.UtcNow - startTime < timeout)
        {
            var history = await GetHistoryAsync(promptResponse.PromptId, cancellationToken);
            
            if (history.Status.Completed)
            {
                return history;
            }

            await Task.Delay(1000, cancellationToken); // Check every second
        }

        throw new TimeoutException($"Workflow execution timed out after {timeout.TotalSeconds} seconds");
    }

    /// <summary>
    /// Gets the WebSocket URL for manual connection
    /// </summary>
    /// <returns>WebSocket URL</returns>
    public string GetWebSocketUrl()
    {
        return _httpService.GetWebSocketUrl(_clientId);
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Generates a unique client ID for this instance
    /// </summary>
    /// <returns>A unique client identifier</returns>
    private static string GenerateClientId()
    {
        return $"ComfySharp-{Guid.NewGuid():N}";
    }

    #endregion

    /// <summary>
    /// Disposes the client and all resources
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            _httpService?.Dispose();
            _webSocketService?.Dispose();
            _disposed = true;
        }
    }
}
