using Xunit;
using ComfySharp.ClientApi.Extensions;
using ComfySharp;
using ComfySharp.ClientApi.Models.Request;

namespace ComfySharp.ClientApi.Tests;

/// <summary>
/// Unit tests for ComfyUIWorkflowExtensions
/// </summary>
public class ComfyUIWorkflowExtensionsTests
{
    [Fact]
    public void ToPromptRequest_WithWorkflow_ShouldCreateValidRequest()
    {
        // Arrange
        var workflow = new ComfyUIWorkflow();
        var clientId = "test-client";
        
        // Act
        var request = workflow.ToPromptRequest(clientId);
        
        // Assert
        Assert.NotNull(request);
        Assert.Equal(clientId, request.ClientId);
        Assert.NotNull(request.Prompt);
    }
    
    [Fact]
    public void ToPromptRequest_WithoutClientId_ShouldCreateRequestWithNullClientId()
    {
        // Arrange
        var workflow = new ComfyUIWorkflow();
        
        // Act
        var request = workflow.ToPromptRequest();
        
        // Assert
        Assert.NotNull(request);
        Assert.Null(request.ClientId);
        Assert.NotNull(request.Prompt);
    }
    
    [Fact]
    public void ToPromptRequest_WithExtraData_ShouldIncludeExtraData()
    {
        // Arrange
        var workflow = new ComfyUIWorkflow();
        var extraData = new Dictionary<string, object>
        {
            ["key1"] = "value1",
            ["key2"] = 123
        };
        
        // Act
        var request = workflow.ToPromptRequest("test-client", extraData);
        
        // Assert
        Assert.NotNull(request);
        Assert.Equal("test-client", request.ClientId);
        Assert.Equal(extraData, request.ExtraData);
    }
    
    [Fact]
    public void ToPromptRequest_WithNullWorkflow_ShouldThrowArgumentNullException()
    {
        // Arrange
        ComfyUIWorkflow? workflow = null;
        
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => workflow!.ToPromptRequest());
    }
}
