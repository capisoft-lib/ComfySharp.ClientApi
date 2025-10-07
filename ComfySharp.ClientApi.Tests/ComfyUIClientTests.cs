using Xunit;
using ComfySharp.ClientApi;
using ComfySharp;

namespace ComfySharp.ClientApi.Tests;

/// <summary>
/// Unit tests for ComfyUIClient
/// </summary>
public class ComfyUIClientTests
{
    [Fact]
    public void Constructor_WithBaseUrl_ShouldInitialize()
    {
        // Arrange & Act
        var client = new ComfyUIClient("http://localhost:8188");
        
        // Assert
        Assert.NotNull(client);
        Assert.NotNull(client.ClientId);
        Assert.StartsWith("ComfySharp-", client.ClientId);
    }
    
    [Fact]
    public void Constructor_WithCustomClientId_ShouldUseProvidedId()
    {
        // Arrange
        var customId = "my-custom-client-id";
        
        // Act
        var client = new ComfyUIClient("http://localhost:8188", clientId: customId);
        
        // Assert
        Assert.Equal(customId, client.ClientId);
    }
    
    [Fact]
    public void Constructor_WithHttpClient_ShouldInitialize()
    {
        // Arrange
        var httpClient = new HttpClient();
        
        // Act
        var client = new ComfyUIClient("http://localhost:8188", httpClient);
        
        // Assert
        Assert.NotNull(client);
        Assert.NotNull(client.ClientId);
    }
    
    [Fact]
    public void GetWebSocketUrl_ShouldReturnValidUrl()
    {
        // Arrange
        var client = new ComfyUIClient("http://localhost:8188");
        
        // Act
        var url = client.GetWebSocketUrl();
        
        // Assert
        Assert.StartsWith("ws://localhost:8188/ws", url);
        Assert.Contains($"client_id={client.ClientId}", url);
    }
    
    [Fact]
    public void GetWebSocketUrl_WithHttps_ShouldReturnWssUrl()
    {
        // Arrange
        var client = new ComfyUIClient("https://localhost:8188");
        
        // Act
        var url = client.GetWebSocketUrl();
        
        // Assert
        Assert.StartsWith("wss://localhost:8188/ws", url);
    }
    
    [Fact]
    public void IsWebSocketConnected_Initially_ShouldBeFalse()
    {
        // Arrange
        var client = new ComfyUIClient("http://localhost:8188");
        
        // Act & Assert
        Assert.False(client.IsWebSocketConnected);
    }
    
    [Fact]
    public void Dispose_ShouldNotThrow()
    {
        // Arrange
        var client = new ComfyUIClient("http://localhost:8188");
        
        // Act & Assert
        client.Dispose();
        // Should not throw
    }
}
