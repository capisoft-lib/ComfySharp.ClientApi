using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;
using ComfySharp.ClientApi;
using ComfySharp.ClientApi.Models.Request;
using ComfySharp.ClientApi.Models.Response;
using ComfySharp;

namespace ComfySharp.ClientApi.Tests;

public class ComfyUIClientIntegrationTests
{
    private static string GetBaseUrl()
    {
        var url = Environment.GetEnvironmentVariable("COMFY_BASE_URL");
        return string.IsNullOrWhiteSpace(url) ? "http://localhost:8188" : url;
    }

    private static string GetBasicWorkflowJson()
    {
        var baseDir = AppContext.BaseDirectory;
        var jsonPath = Path.Combine(baseDir, "basic_workflow.json");
        if (!File.Exists(jsonPath))
        {
            jsonPath = Path.Combine(baseDir, "..", "..", "..", "basic_workflow.json");
        }
        return File.ReadAllText(jsonPath);
    }

    [Fact]
    public async Task QueuePrompt_And_GetHistory_Should_Work_When_Server_Available()
    {
        var baseUrl = GetBaseUrl();

        var client = new ComfyUIClient(baseUrl);

        var workflow = new ComfyUIWorkflow();
        workflow.LoadFromJson(GetBasicWorkflowJson());
        var response = await client.QueuePromptAsync(workflow);

        Assert.False(string.IsNullOrWhiteSpace(response.PromptId));

        // poll history with simple backoff
        HistoryResponse? history = null;
        for (var i = 0; i < 10; i++)
        {
            await Task.Delay(300);
            history = await client.GetHistoryAsync(response.PromptId!);
            if (history != null && (history.Outputs != null && history.Outputs.Count > 0 || history.Status != null && history.Status.Completed))
            {
                break;
            }
        }

        Assert.NotNull(history);
    }

    [Fact]
    public async Task WebSocket_Subscribe_Should_Receive_Status_When_Server_Available()
    {
        var baseUrl = GetBaseUrl();

        var client = new ComfyUIClient(baseUrl);
        await client.ConnectWebSocketAsync();

        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var sub = client.SubscribeStatus(_ =>
        {
            tcs.TrySetResult(_.Finished);
        });

        // queue a quick prompt to trigger messages
        var workflow = new ComfyUIWorkflow();
        workflow.LoadFromJson(GetBasicWorkflowJson());
        var _ = await client.QueuePromptAsync(workflow);

        var timeoutMs = int.TryParse(Environment.GetEnvironmentVariable("COMFY_TIMEOUT_MS"), out var ms) ? ms : 10000;
        var completed = await Task.WhenAny(tcs.Task, Task.Delay(timeoutMs)) == tcs.Task;
        Assert.True(completed);
    }
}


