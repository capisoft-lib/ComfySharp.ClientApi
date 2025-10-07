using Xunit;
using ComfySharp.ClientApi.WebSocket;
using ComfySharp.ClientApi.Models.Response;
using System.Text.Json;

namespace ComfySharp.ClientApi.Tests;

/// <summary>
/// Unit tests for WebSocketMessageDispatcher
/// </summary>
public class WebSocketMessageDispatcherTests
{
    private readonly JsonSerializerOptions _jsonOptions;
    
    public WebSocketMessageDispatcherTests()
    {
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            WriteIndented = false
        };
    }
    
    [Fact]
    public void Subscribe_WithProgressHandler_ShouldReturnDisposable()
    {
        // Arrange
        var dispatcher = new WebSocketMessageDispatcher(_jsonOptions);
        var receivedMessages = new List<ProgressMessage>();
        
        // Act
        var subscription = dispatcher.Subscribe<ProgressMessage>(msg => receivedMessages.Add(msg));
        
        // Assert
        Assert.NotNull(subscription);
        Assert.IsAssignableFrom<IDisposable>(subscription);
    }
    
    [Fact]
    public void Subscribe_WithStatusHandler_ShouldReturnDisposable()
    {
        // Arrange
        var dispatcher = new WebSocketMessageDispatcher(_jsonOptions);
        var receivedMessages = new List<StatusMessage>();
        
        // Act
        var subscription = dispatcher.Subscribe<StatusMessage>(msg => receivedMessages.Add(msg));
        
        // Assert
        Assert.NotNull(subscription);
        Assert.IsAssignableFrom<IDisposable>(subscription);
    }
    
    [Fact]
    public void SubscribeRaw_WithHandler_ShouldReturnDisposable()
    {
        // Arrange
        var dispatcher = new WebSocketMessageDispatcher(_jsonOptions);
        var receivedMessages = new List<WebSocketMessage>();
        
        // Act
        var subscription = dispatcher.SubscribeRaw(msg => receivedMessages.Add(msg));
        
        // Assert
        Assert.NotNull(subscription);
        Assert.IsAssignableFrom<IDisposable>(subscription);
    }
    
    [Fact]
    public void Dispatch_WithProgressMessage_ShouldCallProgressHandlers()
    {
        // Arrange
        var dispatcher = new WebSocketMessageDispatcher(_jsonOptions);
        var receivedMessages = new List<ProgressMessage>();
        var subscription = dispatcher.Subscribe<ProgressMessage>(msg => receivedMessages.Add(msg));
        
        var progressMessage = new WebSocketMessage
        {
            Type = "progress",
            Data = new ProgressMessage
            {
                Step = 1,
                TotalSteps = 10,
                StepDescription = "Processing"
            }
        };
        
        // Act
        dispatcher.Dispatch(progressMessage);
        
        // Assert
        Assert.Single(receivedMessages);
        Assert.Equal(1, receivedMessages[0].Step);
        Assert.Equal(10, receivedMessages[0].TotalSteps);
        Assert.Equal("Processing", receivedMessages[0].StepDescription);
        
        subscription.Dispose();
    }
    
    [Fact]
    public void Dispatch_WithStatusMessage_ShouldCallStatusHandlers()
    {
        // Arrange
        var dispatcher = new WebSocketMessageDispatcher(_jsonOptions);
        var receivedMessages = new List<StatusMessage>();
        var subscription = dispatcher.Subscribe<StatusMessage>(msg => receivedMessages.Add(msg));
        
        var statusMessage = new WebSocketMessage
        {
            Type = "status",
            Data = new StatusMessage
            {
                Status = new StatusInfo
                {
                    ExecInfo = new ExecInfo
                    {
                        QueueRemaining = 1
                    }
                }
            }
        };
        
        // Act
        dispatcher.Dispatch(statusMessage);
        
        // Assert
        Assert.Single(receivedMessages);
        Assert.NotNull(receivedMessages[0].Status);
        Assert.NotNull(receivedMessages[0].Status!.ExecInfo);
        Assert.Equal(1, receivedMessages[0].Status!.ExecInfo!.QueueRemaining);
        
        subscription.Dispose();
    }
    
    [Fact]
    public void Dispatch_WithRawMessage_ShouldCallRawHandlers()
    {
        // Arrange
        var dispatcher = new WebSocketMessageDispatcher(_jsonOptions);
        var receivedMessages = new List<WebSocketMessage>();
        var subscription = dispatcher.SubscribeRaw(msg => receivedMessages.Add(msg));
        
        var rawMessage = new WebSocketMessage
        {
            Type = "custom",
            Data = "custom data",
            PromptId = "test-prompt"
        };
        
        // Act
        dispatcher.Dispatch(rawMessage);
        
        // Assert
        Assert.Single(receivedMessages);
        Assert.Equal("custom", receivedMessages[0].Type);
        Assert.Equal("custom data", receivedMessages[0].Data);
        Assert.Equal("test-prompt", receivedMessages[0].PromptId);
        
        subscription.Dispose();
    }
    
    [Fact]
    public void Dispose_Subscription_ShouldUnsubscribe()
    {
        // Arrange
        var dispatcher = new WebSocketMessageDispatcher(_jsonOptions);
        var receivedMessages = new List<ProgressMessage>();
        var subscription = dispatcher.Subscribe<ProgressMessage>(msg => receivedMessages.Add(msg));
        
        var progressMessage = new WebSocketMessage
        {
            Type = "progress",
            Data = new ProgressMessage { Step = 1, TotalSteps = 10 }
        };
        
        // Act
        subscription.Dispose();
        dispatcher.Dispatch(progressMessage);
        
        // Assert
        Assert.Empty(receivedMessages);
    }
}
