# ComfySharp.ClientApi

A .NET client library for interacting with ComfyUI API endpoints. This library provides strongly-typed models, HTTP client functionality, and real-time WebSocket communication for all ComfyUI operations.

## Features

- **Complete API Coverage**: Support for all ComfyUI endpoints including WebSocket connections
- **Strongly Typed Models**: Request and Response models for all API operations
- **Automatic Client ID Management**: No need to manually provide client IDs
- **Real-time WebSocket Communication**: Subscribe to progress, status, and custom messages
- **Workflow Integration**: Direct support for `ComfyUIWorkflow` objects
- **Async/Await Support**: Modern async programming patterns
- **Error Handling**: Comprehensive error handling and status code management
- **File Upload Support**: Built-in support for image uploads and file management

## Supported Endpoints

- **WebSocket Connection** (`/ws`) - Real-time workflow monitoring with typed subscriptions
- **Prompt Execution** (`/prompt`) - Queue and execute workflows directly from `ComfyUIWorkflow`
- **History Retrieval** (`/history/{prompt_id}`) - Get execution results
- **File Operations** (`/view`, `/upload/image`) - Image retrieval and upload
- **Metadata** (`/object_info`) - Node information and definitions
- **Queue Management** (`/queue`) - Queue status and monitoring
- **Execution Control** (`/interrupt`) - Interrupt running workflows

## Quick Start

### Basic Usage

```csharp
using ComfySharp.ClientApi;
using ComfySharp;

// Initialize the client (auto-generates client ID)
var client = new ComfyUIClient("http://localhost:8188");

// Load and execute a workflow
var workflow = new ComfyUIWorkflow("path/to/workflow.json");
var response = await client.QueuePromptAsync(workflow);

// Get execution results
var history = await client.GetHistoryAsync(response.PromptId);
```

### Real-time Monitoring

```csharp
// Connect to WebSocket for real-time updates
await client.ConnectWebSocketAsync();

// Subscribe to progress updates
var progressSub = client.SubscribeProgress(progress => 
{
    Console.WriteLine($"Step {progress.Step}/{progress.TotalSteps}: {progress.StepDescription}");
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

// Execute workflow with real-time monitoring
var response = await client.QueuePromptAsync(workflow);

// Clean up subscriptions when done
progressSub.Dispose();
statusSub.Dispose();
```

### Advanced Usage

```csharp
// Custom client ID
var client = new ComfyUIClient("http://localhost:8188", clientId: "my-custom-id");

// Upload images
var imageData = File.ReadAllBytes("input.png");
var uploadResult = await client.UploadImageAsync(imageData, "my_image.png");

// Get node information
var nodeInfo = await client.GetObjectInfoAsync("CheckpointLoaderSimple");

// Queue management
var queue = await client.GetQueueAsync();
Console.WriteLine($"Running: {queue.QueueRunning.Count}, Pending: {queue.QueuePending.Count}");

// Interrupt execution
await client.InterruptAsync();
```

## WebSocket Subscriptions

The library provides elegant subscription-based WebSocket communication:

```csharp
// Typed subscriptions with automatic unsubscription
var progressSub = client.SubscribeProgress(handler);
var statusSub = client.SubscribeStatus(handler);
var rawSub = client.SubscribeRaw(handler);

// Generic typed subscriptions
var customSub = client.Subscribe<MyCustomMessage>(handler);

// Unsubscribe when done
progressSub.Dispose();
statusSub.Dispose();
rawSub.Dispose();
customSub.Dispose();
```

## Installation

```bash
dotnet add package ComfySharp.ClientApi
```

## Project Structure

The ComfySharp.ClientApi solution includes:

- **`ComfySharp.ClientApi`** - Main library with all API functionality
- **`ComfySharp.ClientApi.Example`** - Console application demonstrating usage
- **`ComfySharp.ClientApi.Tests`** - Unit tests for all components

### Running the Example

```bash
# Navigate to the example project
cd ComfySharp.ClientApi.Example

# Run the example (requires ComfyUI server running)
dotnet run
```

### Running Tests

```bash
# Navigate to the test project
cd ComfySharp.ClientApi.Tests

# Run all tests
dotnet test
```

## Dependencies

- **ComfySharp**: Core workflow functionality
- **System.Text.Json**: JSON serialization
- **Microsoft.Extensions.Http**: HTTP client extensions
- **Microsoft.Extensions.Logging.Abstractions**: Logging abstractions

## Requirements

- .NET 6.0 or later
- ComfyUI server running and accessible

## API Reference

### ComfyUIClient

The main client class providing access to all ComfyUI functionality.

#### Constructor
```csharp
ComfyUIClient(string baseUrl, HttpClient? httpClient = null, string? clientId = null)
```

#### Key Methods
- `QueuePromptAsync(ComfyUIWorkflow workflow)` - Execute workflow directly
- `QueuePromptAsync(PromptRequest request)` - Execute with custom request
- `ConnectWebSocketAsync()` - Connect for real-time updates
- `SubscribeProgress(Action<ProgressMessage> handler)` - Progress updates
- `SubscribeStatus(Action<StatusMessage> handler)` - Status updates
- `GetHistoryAsync(string promptId)` - Get execution results
- `UploadImageAsync(byte[] imageData, string? filename)` - Upload images
- `GetImageAsync(string filename)` - Download images

### Models

#### Request Models
- `PromptRequest` - Workflow execution request
- `ImageUploadRequest` - Image upload request
- `ViewImageRequest` - Image retrieval request

#### Response Models
- `PromptResponse` - Execution queue response
- `HistoryResponse` - Execution history and results
- `ProgressMessage` - Real-time progress updates
- `StatusMessage` - Real-time status updates
- `QueueResponse` - Queue status information

## License

MIT License - see LICENSE file for details.
