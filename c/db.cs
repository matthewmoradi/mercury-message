using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Text;
using System.Collections.Generic;
using Newtonsoft.Json;
using mercury.model;
using mercury.business;

namespace mercury.controller
{
    public class ctrl_db : Controller
    {
        #region users
        public static user validate_user(HttpRequest req)
        {
            string user_id = ctrl_tools.header_get(req, "user_id");
            string user_token = ctrl_tools.header_get(req, "token");
            return validate_user(user_id, user_token);
        }
        public static user validate_user(string user_id, string user_token)
        {
            try
            {
                string srv_url = _io._config_value("url_users") + "validate";
                string srv_token = _io._config_value("token_users");
                Dictionary<string, string> data = new() { { "user_id", user_id }, { "user_token", user_token }, { "token", srv_token } };
                var res = api.post(srv_url, JsonConvert.SerializeObject(data));
                if (res == null)
                {
                    return null;
                }
                dto.msg msg = JsonConvert.DeserializeObject<dto.msg>(res);
                if (string.IsNullOrEmpty(msg.data))
                    return null;
                var user = JsonConvert.DeserializeObject<user>(msg.data);
                return user;
            }
            catch (Exception ex)
            {
                _sys.log(ex);
                return null;
            }
        }
        #endregion users

        #region messages
        public static dto.msg push_message(string path, message item = null)
        {
            string url = _io._config_value("url_database") + path;
            string token = _io._config_value("token_database");
            Dictionary<string, string> data = new() { { "token", token }, { "action", "messages" } };
            if (item != null)
            {
                string item_str = JsonConvert.SerializeObject(item);
                data.Add("message", item_str);
            }
            var res = api.post(url, JsonConvert.SerializeObject(data));
            if (res == null)
            {
                return null;
            }
            dto.msg msg = JsonConvert.DeserializeObject<dto.msg>(res);
            return msg;
        }
        public static List<message> get_messages()
        {
            dto.msg msg = push_message("get");
            if (msg.success)
                return JsonConvert.DeserializeObject<List<message>>(msg.data);
            return null;
        }
        public static message insert_message(message item)
        {
            dto.msg msg = push_message("insert", item);
            if (msg.success)
                return item;
            return null;
        }
        public static message update_message(message item)
        {
            dto.msg msg = push_message("update", item);
            if (msg.success)
                return item;
            return null;
        }
        public static message delete_message(message item)
        {
            dto.msg msg = push_message("delete", item);
            if (msg.success)
                return item;
            return null;
        }
        #endregion messages

        #region chats
        public static dto.msg push_chat(string path, chat item = null)
        {
            string url = _io._config_value("url_database") + path;
            string token = _io._config_value("token_database");
            Dictionary<string, string> data = new() { { "token", token }, { "action", "chats" } };
            if (item != null)
            {
                string item_str = JsonConvert.SerializeObject(item);
                data.Add("chat", item_str);
            }
            var res = api.post(url, JsonConvert.SerializeObject(data));
            if (res == null)
            {
                return null;
            }
            dto.msg msg = JsonConvert.DeserializeObject<dto.msg>(res);
            return msg;
        }
        public static List<chat> get_chats()
        {
            dto.msg msg = push_chat("get");
            if (msg.success)
                return JsonConvert.DeserializeObject<List<chat>>(msg.data);
            return null;
        }
        public static chat insert_chat(chat item)
        {
            dto.msg msg = push_chat("insert", item);
            if (msg.success)
                return item;
            return null;
        }
        public static chat update_chat(chat item)
        {
            dto.msg msg = push_chat("update", item);
            if (msg.success)
                return item;
            return null;
        }
        public static chat delete_chat(chat item)
        {
            dto.msg msg = push_chat("delete", item);
            if (msg.success)
                return item;
            return null;
        }
        #endregion chats

        #region get readonly
        public static dto.msg push(string path, string action)
        {
            string url = _io._config_value("url_database") + path;
            string token = _io._config_value("token_database");
            Dictionary<string, string> data = new() { { "token", token }, { "action", action } };
            var res = api.post(url, JsonConvert.SerializeObject(data));
            if (res == null)
            {
                return null;
            }
            dto.msg msg = JsonConvert.DeserializeObject<dto.msg>(res);
            return msg;
        }
        public static List<user> get_users()
        {
            dto.msg msg = push("get", "users");
            if (msg.success)
                return JsonConvert.DeserializeObject<List<user>>(msg.data);
            return null;
        }
        public static List<group> get_groups()
        {
            dto.msg msg = push("get", "get_groups");
            if (msg.success)
                return JsonConvert.DeserializeObject<List<group>>(msg.data);
            return null;
        }
        public static List<group_user> get_group_users()
        {
            dto.msg msg = push("get", "group_users");
            if (msg.success)
                return JsonConvert.DeserializeObject<List<group_user>>(msg.data);
            return null;
        }
        #endregion readonly
    }
}