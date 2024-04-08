using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using mercury.business;
using mercury.data;
using mercury.model;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace mercury.controller
{
    public static class ws_handle
    {
        public static ConcurrentDictionary<string, user_min> USERS = new ConcurrentDictionary<string, user_min>();
        public static async void handle(string socket_id, string msg)
        {
            //Console.WriteLine(msg);
            var ws_msg = JsonConvert.DeserializeObject<dto.msg_ws_recv>(msg);
            if (string.IsNullOrEmpty(ws_msg.user_id) || string.IsNullOrEmpty(ws_msg.token))
            {
                System.Console.WriteLine("wrong params");
                return;
            }
            var _user = ctrl_db.validate_user(ws_msg.user_id, ws_msg.token);
            if (_user == null)
            {
                System.Console.WriteLine("user not found or not authed");
                return;
            }
            user_min _user_min = new user_min(_user, socket_id);
            if (USERS.ContainsKey(socket_id))
                _user_min = USERS[socket_id];
            else
                USERS.TryAdd(socket_id, _user_min);
            if (string.IsNullOrEmpty(ws_msg.action))
            {
                await ws.send(_user_min, dto.msg.error_500());
                return;
            }
            if (ws_msg.action == "auth")
            {
                await ws.send(_user_min, dto.msg.success_data(JsonConvert.SerializeObject(dto.msg_ws.authed())));
                return;
            }
            if (string.IsNullOrEmpty(ws_msg.data))
            {
                await ws.send(_user_min, dto.msg.error_500());
                return;
            }
            var parameters_inner = JsonConvert.DeserializeObject<Dictionary<string, string>>(ws_msg.data);
            switch (ws_msg.action)
            {
                case "message":
                    return;

            }
            await Task.FromResult<object>(null);
        }
        public static async void dispose(string socket_id)
        {
            // code here
            await Task.FromResult<object>(null);
        }
        // 
        public static user_min USER_(string user_id)
        {
            return USERS.Where(x => x.Value.id == user_id).Select(x => x.Value).FirstOrDefault();
        }
    }
}