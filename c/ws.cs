using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using mercury.model;
using mercury.business;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace mercury.controller
{
    public class ws
    {
        public static ConcurrentDictionary<string, WebSocket> _SOCKETS = new ConcurrentDictionary<string, WebSocket>();
        private readonly RequestDelegate _next;
        public ws(RequestDelegate next)
        {
            _next = next;
        }
        public async Task Invoke(HttpContext context)
        {
            if (!context.WebSockets.IsWebSocketRequest)
            {
                await _next.Invoke(context);
                return;
            }

            CancellationToken ct = context.RequestAborted;
            WebSocket currentSocket = await context.WebSockets.AcceptWebSocketAsync();
            //var socketId = Guid.NewGuid ().ToString ();
            var socketId = DateTime.Now.Ticks.ToString();
            _SOCKETS.TryAdd(socketId, currentSocket);
            while (true)
            {
                if (ct.IsCancellationRequested)
                {
                    break;
                }
                var response = await recv(currentSocket, ct);
                if (string.IsNullOrEmpty(response))
                {
                    if (currentSocket.State != WebSocketState.Open)
                        break;
                    continue;
                }
                ws_handle.handle(socketId, response);
            }
            await close(socketId, ct);
        }
        public static Task close(string socketId, CancellationToken ct)
        {
            ws_handle.dispose(socketId);
            WebSocket dummy;
            _SOCKETS.TryRemove(socketId, out dummy);
            if (dummy == null)
                return Task.FromResult<object>(null);
            dummy.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", ct);
            dummy.Dispose();
            Console.WriteLine("Connection Closed: " + socketId);
            return Task.FromResult<object>(null);
        }
        public static Task send(user_min _user_min, dto.msg msg)
        {
            return send(_user_min.socket_id, _user_min.k, msg);
        }
        public static Task send(string socket_id, string k, string code, string str, string data)
        {
            return send(socket_id, k, new dto.msg(code, str, data));
        }
        public static Task send(user_min _user_min, string code, string str, string data)
        {
            return send(_user_min.socket_id, _user_min.k, code, str, data);
        }
        public static Task send(string socket_id, string k, dto.msg msg)
        {
            if (_SOCKETS.ContainsKey(socket_id))
                return send(_SOCKETS[socket_id], JsonConvert.SerializeObject(msg));
                // return send(_SOCKETS[socket_id], stringify.encrypt(JsonConvert.SerializeObject(msg), k));
            return Task.FromResult<object>(null);
        }
        private static Task send(WebSocket socket, string data, CancellationToken ct = default(CancellationToken))
        {
            //Console.WriteLine ("send: " + data);
            try
            {
                if (socket.State != WebSocketState.Open)
                {
                    //Console.WriteLine("WebSocketState: " + socket.State + " ,data: " + data);
                    string key = _SOCKETS.Where(x => x.Value == socket).Select(x => x.Key).FirstOrDefault();
                    if (!string.IsNullOrEmpty(key))
                        close(key, ct);
                    return Task.FromResult<object>(null);
                }
                var buffer = Encoding.UTF8.GetBytes(data);
                var segment = new ArraySegment<byte>(buffer);
                return socket.SendAsync(segment, WebSocketMessageType.Text, true, ct);
            }
            catch (Exception x)
            {
                Console.WriteLine(x.Message);
                return Task.FromResult<object>(null);
            }
        }
        private static async Task<string> recv(WebSocket socket, CancellationToken ct = default(CancellationToken))
        {
            var buffer = new ArraySegment<byte>(new byte[8192]);
            using (var ms = new MemoryStream())
            {
                WebSocketReceiveResult result;
                do
                {
                    ct.ThrowIfCancellationRequested();
                    result = await socket.ReceiveAsync(buffer, ct);
                    ms.Write(buffer.Array, buffer.Offset, result.Count);
                }
                while (!result.EndOfMessage);
                ms.Seek(0, SeekOrigin.Begin);
                if (result.MessageType != WebSocketMessageType.Text)
                    return null;
                using (var reader = new StreamReader(ms, Encoding.UTF8))
                {
                    return await reader.ReadToEndAsync();
                }
            }
        }
    }
}