using ComfySharp.ClientApi;
using ComfySharp;

namespace ComfySharp.ClientApi.Example;

/// <summary>
/// Example program demonstrating ComfySharp.ClientApi usage
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("ComfySharp.ClientApi Example");
        Console.WriteLine("============================");
        
        // Initialize the client
        var client = new ComfyUIClient("http://localhost:8188");
        Console.WriteLine($"Client ID: {client.ClientId}");
        
        try
        {
            // Example 1: Basic workflow execution
            await BasicWorkflowExample(client);
            
            // Example 2: Real-time monitoring
            await RealTimeMonitoringExample(client);
            
            // Example 3: File operations
            await FileOperationsExample(client);
            
            // Example 4: Queue management
            await QueueManagementExample(client);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            client.Dispose();
        }
    }
    
    /// <summary>
    /// Example of basic workflow execution
    /// </summary>
    static async Task BasicWorkflowExample(ComfyUIClient client)
    {
        Console.WriteLine("\n--- Basic Workflow Example ---");
        
        // Load workflow from file
        var workflow = new ComfyUIWorkflow("basic_workflow.json");
        Console.WriteLine($"Loaded workflow with {workflow.NodeCount} nodes");
        
        // Queue the workflow
        var response = await client.QueuePromptAsync(workflow);
        Console.WriteLine($"Queued prompt with ID: {response.PromptId}");
        
        // Wait a bit for execution
        await Task.Delay(2000);
        
        // Get execution history
        var history = await client.GetHistoryAsync(response.PromptId);
        Console.WriteLine($"Execution status: {history.Status.StatusStr}");
        Console.WriteLine($"Completed: {history.Status.Completed}");
    }
    
    /// <summary>
    /// Example of real-time monitoring with WebSocket
    /// </summary>
    static async Task RealTimeMonitoringExample(ComfyUIClient client)
    {
        Console.WriteLine("\n--- Real-time Monitoring Example ---");
        
        // Connect to WebSocket
        await client.ConnectWebSocketAsync();
        Console.WriteLine("Connected to WebSocket");
        
        // Subscribe to progress updates
        var progressSub = client.SubscribeProgress(progress =>
        {
            Console.WriteLine($"Progress: {progress.Step}/{progress.TotalSteps} - {progress.StepDescription}");
        });
        
        // Subscribe to status updates
        var statusSub = client.SubscribeStatus(status =>
        {
            Console.WriteLine($"Status: {status.Status}");
            if (status.Outputs != null)
            {
                Console.WriteLine($"Outputs: {string.Join(", ", status.Outputs.Keys)}");
            }
        });
        
        // Execute workflow with monitoring
        var workflow = new ComfyUIWorkflow("basic_workflow.json");
        var response = await client.QueuePromptAsync(workflow);
        Console.WriteLine($"Executing workflow with ID: {response.PromptId}");
        
        // Wait for completion
        await Task.Delay(5000);
        
        // Clean up subscriptions
        progressSub.Dispose();
        statusSub.Dispose();
        
        await client.DisconnectWebSocketAsync();
        Console.WriteLine("Disconnected from WebSocket");
    }
    
    /// <summary>
    /// Example of file operations
    /// </summary>
    static async Task FileOperationsExample(ComfyUIClient client)
    {
        Console.WriteLine("\n--- File Operations Example ---");
        
        // Upload an image (if you have one)
        try
        {
            var imageData = File.ReadAllBytes("input.png");
            var uploadResult = await client.UploadImageAsync(imageData, "example_input.png");
            Console.WriteLine($"Uploaded image: {uploadResult.Name}");
        }
        catch (FileNotFoundException)
        {
            Console.WriteLine("No input.png file found, skipping upload example");
        }
        
        // Get node information
        var nodeInfo = await client.GetObjectInfoAsync("CheckpointLoaderSimple");
        Console.WriteLine($"Found {nodeInfo.Count} node types");
        
        // List some available nodes
        foreach (var node in nodeInfo.Take(5))
        {
            Console.WriteLine($"- {node.Key}: {node.Value.Name}");
        }
    }
    
    /// <summary>
    /// Example of queue management
    /// </summary>
    static async Task QueueManagementExample(ComfyUIClient client)
    {
        Console.WriteLine("\n--- Queue Management Example ---");
        
        // Get current queue status
        var queue = await client.GetQueueAsync();
        Console.WriteLine($"Queue Status:");
        Console.WriteLine($"- Running: {queue.QueueRunning.Count}");
        Console.WriteLine($"- Pending: {queue.QueuePending.Count}");
        
        // Show running prompts
        foreach (var item in queue.QueueRunning)
        {
            Console.WriteLine($"  Running: {item.PromptId} - {item.Status.StatusStr}");
        }
        
        // Show pending prompts
        foreach (var item in queue.QueuePending)
        {
            Console.WriteLine($"  Pending: {item.PromptId} - {item.Status.StatusStr}");
        }
    }
}
