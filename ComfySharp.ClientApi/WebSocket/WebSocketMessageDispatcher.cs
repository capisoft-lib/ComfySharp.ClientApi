using System.Collections.Concurrent;
using System.Text.Json;
using ComfySharp.ClientApi.Models.Response;

namespace ComfySharp.ClientApi.WebSocket;

/// <summary>
/// Dispatches WebSocket messages to typed subscribers.
/// </summary>
public class WebSocketMessageDispatcher
{
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly ConcurrentDictionary<Type, List<IDisposable>> _subscriptions = new();
    private readonly List<IDisposable> _rawSubscriptions = new();

    public WebSocketMessageDispatcher(JsonSerializerOptions jsonOptions)
    {
        _jsonOptions = jsonOptions;
    }

    public IDisposable Subscribe<T>(Action<T> handler) where T : class
    {
        var list = _subscriptions.GetOrAdd(typeof(T), _ => new List<IDisposable>());
        // Pass the subscription into the onDispose closure so removal uses the exact instance
        var sub = new TypedSubscription<T>(handler, s => Remove(typeof(T), s));
        lock (list)
        {
            list.Add(sub);
        }
        return sub;
    }

    public IDisposable SubscribeRaw(Action<WebSocketMessage> handler)
    {
        // Pass the subscription into the onDispose closure so removal uses the exact instance
        var sub = new RawSubscription(handler, s => Remove(null, s));
        lock (_rawSubscriptions)
        {
            _rawSubscriptions.Add(sub);
        }
        return sub;
    }

    private void Remove(Type? type, IDisposable? sub)
    {
        if (type != null)
        {
            if (_subscriptions.TryGetValue(type, out var list))
            {
                lock (list)
                {
                    if (sub != null)
                    {
                        list.RemoveAll(s => s == sub);
                    }
                }
            }
        }
        else
        {
            lock (_rawSubscriptions)
            {
                if (sub != null)
                {
                    _rawSubscriptions.RemoveAll(s => s == sub);
                }
            }
        }
    }

    public void Dispatch(WebSocketMessage message)
    {
        // Raw first
        List<IDisposable> raw;
        lock (_rawSubscriptions)
        {
            raw = _rawSubscriptions.ToList();
        }
        foreach (var s in raw)
        {
            if (s is RawSubscription rs)
            {
                rs.Handler(message);
            }
        }

        if (message.Type == null || message.Data == null) return;

        // Route by known types
        Route<ProgressMessage>(message, "progress");
        Route<StatusMessage>(message, "status");
    }

    private void Route<T>(WebSocketMessage message, string type) where T : class
    {
        if (!string.Equals(message.Type, type, StringComparison.OrdinalIgnoreCase)) return;
        if (!_subscriptions.TryGetValue(typeof(T), out var list)) return;
        string json = message.Data is string s ? s : JsonSerializer.Serialize(message.Data, _jsonOptions);
        var payload = JsonSerializer.Deserialize<T>(json, _jsonOptions);
        if (payload == null) return;
        List<IDisposable> snapshot;
        lock (list)
        {
            snapshot = list.ToList();
        }
        foreach (var sub in snapshot)
        {
            if (sub is TypedSubscription<T> ts)
            {
                ts.Handler(payload);
            }
        }
    }

    private sealed class TypedSubscription<T> : IDisposable where T : class
    {
        public readonly Action<T> Handler;
        private readonly Action<IDisposable> _onDispose;
        private bool _disposed;
        public TypedSubscription(Action<T> handler, Action<IDisposable> onDispose)
        {
            Handler = handler;
            _onDispose = onDispose;
        }
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _onDispose(this);
        }
    }

    private sealed class RawSubscription : IDisposable
    {
        public readonly Action<WebSocketMessage> Handler;
        private readonly Action<IDisposable> _onDispose;
        private bool _disposed;
        public RawSubscription(Action<WebSocketMessage> handler, Action<IDisposable> onDispose)
        {
            Handler = handler;
            _onDispose = onDispose;
        }
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _onDispose(this);
        }
    }
}


