using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using ComfySharp.ClientApi.Models.Response;
using ComfySharp.ClientApi.WebSocket;

namespace ComfySharp.ClientApi;

/// <summary>
/// WebSocket service for real-time ComfyUI communication
/// </summary>
public class ComfyUIWebSocketService : IDisposable
{
    private ClientWebSocket? _webSocket;
    private readonly JsonSerializerOptions _jsonOptions;
    private bool _disposed = false;
    private readonly WebSocketMessageDispatcher _dispatcher;

    /// <summary>
    /// Event fired when a progress update is received
    /// </summary>
    public event EventHandler<ProgressMessage>? ProgressReceived;

    /// <summary>
    /// Event fired when a status update is received
    /// </summary>
    public event EventHandler<StatusMessage>? StatusReceived;

    /// <summary>
    /// Event fired when a general message is received
    /// </summary>
    public event EventHandler<WebSocketMessage>? MessageReceived;

    /// <summary>
    /// Event fired when the connection is established
    /// </summary>
    public event EventHandler? Connected;

    /// <summary>
    /// Event fired when the connection is closed
    /// </summary>
    public event EventHandler? Disconnected;

    /// <summary>
    /// Event fired when an error occurs
    /// </summary>
    public event EventHandler<Exception>? ErrorOccurred;

    /// <summary>
    /// Gets whether the WebSocket is connected
    /// </summary>
    public bool IsConnected => _webSocket?.State == WebSocketState.Open;

    /// <summary>
    /// Initializes a new instance of the ComfyUIWebSocketService
    /// </summary>
    public ComfyUIWebSocketService()
    {
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            WriteIndented = false
        };
        _dispatcher = new WebSocketMessageDispatcher(_jsonOptions);
    }

    /// <summary>
    /// Connects to the ComfyUI WebSocket endpoint
    /// </summary>
    /// <param name="url">WebSocket URL</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task ConnectAsync(string url, CancellationToken cancellationToken = default)
    {
        if (_webSocket != null && _webSocket.State == WebSocketState.Open)
        {
            return;
        }

        _webSocket = new ClientWebSocket();
        
        try
        {
            await _webSocket.ConnectAsync(new Uri(url), cancellationToken);
            Connected?.Invoke(this, EventArgs.Empty);
            
            // Start listening for messages
            _ = Task.Run(() => ListenForMessagesAsync(cancellationToken), cancellationToken);
        }
        catch (Exception ex)
        {
            ErrorOccurred?.Invoke(this, ex);
            throw;
        }
    }

    /// <summary>
    /// Disconnects from the WebSocket
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        if (_webSocket?.State == WebSocketState.Open)
        {
            await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Disconnecting", cancellationToken);
        }
        
        Disconnected?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Sends a message through the WebSocket
    /// </summary>
    /// <param name="message">Message to send</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task SendMessageAsync(object message, CancellationToken cancellationToken = default)
    {
        if (_webSocket?.State != WebSocketState.Open)
        {
            throw new InvalidOperationException("WebSocket is not connected");
        }

        var json = JsonSerializer.Serialize(message, _jsonOptions);
        var bytes = Encoding.UTF8.GetBytes(json);
        
        await _webSocket.SendAsync(
            new ArraySegment<byte>(bytes), 
            WebSocketMessageType.Text, 
            true, 
            cancellationToken);
    }

    private async Task ListenForMessagesAsync(CancellationToken cancellationToken)
    {
        var buffer = new byte[16384];
        
        try
        {
            while (_webSocket?.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
            {
                var segment = new ArraySegment<byte>(buffer);
                var result = await _webSocket.ReceiveAsync(segment, cancellationToken);
                
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var messageBuilder = new StringBuilder();
                    messageBuilder.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));

                    while (!result.EndOfMessage)
                    {
                        result = await _webSocket.ReceiveAsync(segment, cancellationToken);
                        messageBuilder.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));
                    }

                    var message = messageBuilder.ToString();
                    await ProcessMessageAsync(message);
                }
                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    break;
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when cancellation is requested
        }
        catch (Exception ex)
        {
            ErrorOccurred?.Invoke(this, ex);
        }
        finally
        {
            Disconnected?.Invoke(this, EventArgs.Empty);
        }
    }

    private Task ProcessMessageAsync(string message)
    {
        try
        {
            var webSocketMessage = JsonSerializer.Deserialize<WebSocketMessage>(message, _jsonOptions);
            if (webSocketMessage == null) return Task.CompletedTask;

            MessageReceived?.Invoke(this, webSocketMessage);
            _dispatcher.Dispatch(webSocketMessage);

            // Process specific message types
            switch (webSocketMessage.Type?.ToLower())
            {
                case "progress":
                    if (webSocketMessage.Data != null)
                    {
                        var dataJson = webSocketMessage.Data is string s ? s : JsonSerializer.Serialize(webSocketMessage.Data, _jsonOptions);
                        var progress = JsonSerializer.Deserialize<ProgressMessage>(dataJson, _jsonOptions);
                        if (progress != null)
                        {
                            ProgressReceived?.Invoke(this, progress);
                        }
                    }
                    break;

                case "status":
                    if (webSocketMessage.Data != null)
                    {
                        var dataJson = webSocketMessage.Data is string ss ? ss : JsonSerializer.Serialize(webSocketMessage.Data, _jsonOptions);
                        var status = JsonSerializer.Deserialize<StatusMessage>(dataJson, _jsonOptions);
                        if (status != null)
                        {
                            StatusReceived?.Invoke(this, status);
                        }
                    }
                    break;
            }
        }
        catch (Exception ex)
        {
            ErrorOccurred?.Invoke(this, ex);
        }
        
        return Task.CompletedTask;
    }

    /// <summary>
    /// Disposes the WebSocket service
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            _webSocket?.Dispose();
            _disposed = true;
        }
    }

    #region Subscriptions
    public IDisposable Subscribe<T>(Action<T> handler) where T : class
    {
        return _dispatcher.Subscribe(handler);
    }

    public IDisposable SubscribeRaw(Action<WebSocketMessage> handler)
    {
        return _dispatcher.SubscribeRaw(handler);
    }

    public IDisposable SubscribeProgress(Action<ProgressMessage> handler)
    {
        return _dispatcher.Subscribe(handler);
    }

    public IDisposable SubscribeStatus(Action<StatusMessage> handler)
    {
        return _dispatcher.Subscribe(handler);
    }
    #endregion
}
