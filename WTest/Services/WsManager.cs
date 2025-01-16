using System.Collections.Concurrent;
using System.Net.WebSockets;

namespace WTest.Services;

public class WsManager
{
    public Dictionary<string, (WebSocket WebSocket, CancellationTokenSource CancellationTokenSource)> WebSockets { get; set; }

    public WsManager()
    {
        this.WebSockets = new Dictionary<string, (WebSocket WebSocket, CancellationTokenSource CancellationTokenSource)>();
    }

}
