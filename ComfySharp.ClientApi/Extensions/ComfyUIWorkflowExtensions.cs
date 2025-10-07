using System.Text.Json;
using ComfySharp;
using ComfySharp.ClientApi.Models.Request;

namespace ComfySharp.ClientApi.Extensions;

/// <summary>
/// Extensions for converting ComfyUIWorkflow to ClientApi request models.
/// </summary>
public static class ComfyUIWorkflowExtensions
{
    /// <summary>
    /// Converts a ComfyUIWorkflow into a PromptRequest ready for /prompt endpoint.
    /// </summary>
    /// <param name="workflow">The workflow instance.</param>
    /// <param name="clientId">Optional client identifier.</param>
    /// <param name="extraData">Optional extra data to attach.</param>
    /// <returns>PromptRequest populated from the workflow.</returns>
    public static PromptRequest ToPromptRequest(this ComfyUIWorkflow workflow, string? clientId = null, Dictionary<string, object>? extraData = null)
    {
        if (workflow == null) throw new ArgumentNullException(nameof(workflow));

        // Get the raw JSON string from the workflow
        var json = workflow.ToJson();
        
        // Deserialize into a JsonElement so it is emitted as a raw JSON object (not quoted)
        var promptElement = JsonSerializer.Deserialize<JsonElement>(json);
        return new PromptRequest
        {
            Prompt = promptElement,
            ClientId = clientId,
            ExtraData = extraData
        };
    }
}


