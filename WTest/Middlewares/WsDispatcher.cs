using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using WTest.Services;

namespace WTest.Middlewares
{
    public class WsDispatcher
    {
        private readonly RequestDelegate _next;
        private readonly WsManager _wsManager;

        public WsDispatcher(RequestDelegate next, WsManager wsManager)
        {
            this._next = next;
            this._wsManager = wsManager;
        }

        public async Task Invoke(HttpContext context)
        {
            var user = context.Session.GetString("user");
            if (user == null || !context.WebSockets.IsWebSocketRequest)
            {
                await _next(context);
                return;
            }
            await DispatchAsync(context, user);
        }

        private async Task DispatchAsync(HttpContext context, string user)
        {
            if (this._wsManager.WebSockets.ContainsKey(user))
            {
                this._wsManager.WebSockets.Remove(user, out var socket);
                socket.CancellationTokenSource.Cancel();
                await socket.WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
            }
            WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
            var cts = new CancellationTokenSource();
            this._wsManager.WebSockets.Add(user, (webSocket, cts));
            await Task.Run(async () => await this.SyncAsync(webSocket, user), cts.Token);
        }

        public async Task SyncAsync(WebSocket ws, string user)
        {
            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            while (!result.CloseStatus.HasValue)
            {
                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                var serverMsg = Encoding.UTF8.GetBytes(user + ": " + message);
                foreach (var w in this._wsManager.WebSockets.Values)
                {
                    await w.WebSocket.SendAsync(new ArraySegment<byte>(serverMsg, 0, serverMsg.Length), result.MessageType, result.EndOfMessage, CancellationToken.None);
                }

                result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }
            await ws.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
            this._wsManager.WebSockets.Remove(user);
        }
    }
}
