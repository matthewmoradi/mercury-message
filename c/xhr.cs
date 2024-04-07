using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using System.Text;
using mercury.business;
using mercury.data;
using mercury.model;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace mercury.controller
{
    public class ctrl_xhr : Controller
    {
        #region root
        [HttpPost]
        [EnableCors("cors_mercury")]
        [Route("/wind")]
        public ActionResult wind()
        {
            string body = new StreamReader(HttpContext.Request.Body, Encoding.UTF8, true, 1024, true).ReadToEnd();
            user _user = ctrl_db.validate_user(Request);
            if (_user == null)
                return ctrl_tools.ret(this, dto.msg.error_unauth());
            Dictionary<string, string> parameters = JsonConvert.DeserializeObject<Dictionary<string, string>>(body);
            if (parameters == null || parameters.Count == 0)
                return ctrl_tools.ret(this, dto.msg.error_500());
            if (!ctrl_tools.contains(parameters, new string[] { "action" }))
                return ctrl_tools.ret(this, dto.msg.error_500());
            switch (parameters["action"])
            {
                // chat
                case "chat_get_or_add":
                    return chat_get_or_add(_user, parameters);
                case "chat_get":
                    return chat_get(_user, parameters);
                case "chat_clear":
                    return chat_clear(_user, parameters);
                case "chat_delete":
                    return chat_delete(_user, parameters);
                // message
                case "message":
                    return message(_user, parameters);
                case "message_edit":
                    return message_edit(_user, parameters);
                case "message_forward":
                    return message_forward(_user, parameters);
                case "message_get":
                    return message_get(_user, parameters);
                case "message_search":
                    return message_search(_user, parameters);
                case "message_delete":
                    return message_delete(_user, parameters);
            }
            return ctrl_tools.ret(this, new dto.msg(dto.msg.response_code_valid, message_sys.action_not_found, "", false));
        }
        #endregion root

        #region chat
        public ActionResult chat_get_or_add(user _user, Dictionary<string, string> parameters)
        {
            if (!ctrl_tools.contains(parameters, new string[] { "id", "username" }))
                return ctrl_tools.ret(this, dto.msg.error_500());
            var item = _chats.get_(parameters["id"]);
            if (item == null)
            {
                var user_target = _commons.users_get__username(parameters["username"]);
                if (user_target == null)
                    return ctrl_tools.ret(this, dto.msg.error_500());
                item = _chats.add(_user.id, user_target.id);
                if (item == null)
                {
                    System.Console.WriteLine("Hain?");
                }
            }
            var res = new chat_dto_client_user(item, _user);
            return ctrl_tools.ret(this, dto.msg.success_data(JsonConvert.SerializeObject(res)));
        }
        public ActionResult chat_get(user _user, Dictionary<string, string> parameters)
        {
            var query = _chats.get_user(_user.id);
            var res = query.Select(x => new chat_dto_client_chat(x, _user)).ToList();
            return ctrl_tools.ret(this, dto.msg.success_data(JsonConvert.SerializeObject(res)));
        }
        public ActionResult chat_clear(user _user, Dictionary<string, string> parameters)
        {
            if (!ctrl_tools.contains(parameters, new string[] { "id" }))
                return ctrl_tools.ret(this, dto.msg.error_500());
            string str = "";
            var res = _chats.clear(_user, parameters["id"], ref str);
            if (!res)
                return ctrl_tools.ret(this, dto.msg.fail_(str));
            return ctrl_tools.ret(this, dto.msg.success_());
        }
        public ActionResult chat_delete(user _user, Dictionary<string, string> parameters)
        {
            if (!ctrl_tools.contains(parameters, new string[] { "id" }))
                return ctrl_tools.ret(this, dto.msg.error_500());
            string str = "";
            var res = _chats.delete(_user, parameters["id"], ref str);
            if (!res)
                return ctrl_tools.ret(this, dto.msg.fail_(str));
            return ctrl_tools.ret(this, dto.msg.success_());
        }

        #endregion chat

        #region message
        public ActionResult message(user _user, Dictionary<string, string> parameters)
        {
            if (!ctrl_tools.contains(parameters, new string[] { "chat_id", "text", "type", "attachment", "reply_id" }))
                return ctrl_tools.ret(this, dto.msg.error_500());
            //check attachment, must be base64
            //add message
            string str = "";
            System.Console.WriteLine(parameters["text"]);
            message _message = _messages.add(_user, parameters["chat_id"], parameters["text"], parameters["reply_id"], parameters["attachment"], int.Parse(parameters["type"]), ref str);
            if (_message == null)
                return ctrl_tools.ret(this, dto.msg.fail_(str));
            return ctrl_tools.ret(this, dto.msg.success_());
        }
        public ActionResult message_files(user _user, Dictionary<string, string> parameters)
        {
            if (!ctrl_tools.contains(parameters, new string[] { "chat_id", "text", "attachments", "reply_id" }))
                return ctrl_tools.ret(this, dto.msg.error_500());
            //check attachments, must be base64
            //add messages
            string str = "";
            var attachments = JsonConvert.DeserializeObject<string[]>(parameters["attachments"]);
            if (attachments.Count() == 0)
            {
                return ctrl_tools.ret(this, dto.msg.fail_());
            }
            if (attachments.Count() > 1)
            {
                if (!string.IsNullOrEmpty(parameters["text"]))
                {
                    _messages.add(_user, parameters["chat_id"], parameters["text"], parameters["reply_id"], parameters["attachment"], int.Parse(parameters["type"]), ref str);
                }
                foreach (string item in attachments)
                {
                    string attachment = stringify.from_base64(item);
                    _messages.add(_user, parameters["chat_id"], "", parameters["reply_id"], attachment, int.Parse(parameters["type"]), ref str);
                }
                return ctrl_tools.ret(this, dto.msg.success_());
            }
            _messages.add(_user, parameters["chat_id"], parameters["text"], parameters["reply_id"], stringify.from_base64(attachments[0]), int.Parse(parameters["type"]), ref str);
            return ctrl_tools.ret(this, dto.msg.success_());
        }

        public ActionResult message_edit(user _user, Dictionary<string, string> parameters)
        {
            if (!ctrl_tools.contains(parameters, new string[] { "id", "text_edited" }))
                return ctrl_tools.ret(this, dto.msg.error_500());
            string str = "";
            bool res = _messages.edit(_user, parameters["id"], parameters["text_edited"], ref str);
            if (!res)
                return ctrl_tools.ret(this, new dto.msg(dto.msg.response_code_valid, str, "", false));
            return ctrl_tools.ret(this, dto.msg.success_());
        }

        public ActionResult message_forward(user _user, Dictionary<string, string> parameters)
        {
            if (!ctrl_tools.contains(parameters, new string[] { "id", "chat_ids" }))
                return ctrl_tools.ret(this, dto.msg.error_500());
            string[] chat_ids = JsonConvert.DeserializeObject<string[]>(parameters["chat_ids"]);
            string str = "";
            bool res = _messages.forward(_user, parameters["id"], chat_ids, ref str);
            if (!res)
                return ctrl_tools.ret(this, new dto.msg(dto.msg.response_code_valid, str, "", false));
            return ctrl_tools.ret(this, dto.msg.success_());
        }
        public ActionResult message_get(user _user, Dictionary<string, string> parameters)
        {
            if (!ctrl_tools.contains(parameters, new string[] { "chat_id", "skip", "take", "dt_from", "dt_to" }))
                return ctrl_tools.ret(this, dto.msg.error_500());
            int skip = int.Parse(parameters["skip"]);
            int take = int.Parse(parameters["take"]);
            long? dt_from = long.Parse(parameters["dt_from"]);
            long? dt_to = long.Parse(parameters["dt_to"]);
            dt_from = dt_from == 0 ? null : dt_from;
            dt_to = dt_to == 0 ? null : dt_to;
            var query = _messages.get(_user.id, parameters["chat_id"], skip, take, dt_from, dt_to);
            var res = query.Select(x => new message_dto(x, _user)).ToList();
            return ctrl_tools.ret(this, dto.msg.success_data(JsonConvert.SerializeObject(res)));
        }
        public ActionResult message_search(user _user, Dictionary<string, string> parameters)
        {
            if (!ctrl_tools.contains(parameters, new string[] { "s", "chat_id" }))
                return ctrl_tools.ret(this, dto.msg.error_500());
            var query = _messages.search(_user.id, parameters["s"], parameters["chat_id"]);
            var res = query.Select(x => new message_dto(x, _user)).ToList();
            return ctrl_tools.ret(this, dto.msg.success_data(JsonConvert.SerializeObject(res)));
        }
        public ActionResult message_delete(user _user, Dictionary<string, string> parameters)
        {
            if (!ctrl_tools.contains(parameters, new string[] { "id" }))
                return ctrl_tools.ret(this, dto.msg.error_500());
            string str = "";
            bool res = _messages.delete(_user, parameters["id"], ref str);
            if (!res)
            {
                return ctrl_tools.ret(this, dto.msg.fail_(str));
            }
            return ctrl_tools.ret(this, dto.msg.success_());
        }
        #endregion message

    }
}