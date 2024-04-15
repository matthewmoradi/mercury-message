using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using mercury.business;
using mercury.data;

namespace mercury.model
{
    public class chat_dto_min
    {
        public string id { get; set; }
        public string user_1_ { get; set; }
        public string user_2_ { get; set; }
        public string group_ { get; set; }
        public chat_dto_min(chat _chat)
        {
            this.id = _chat.id;
            this.user_1_ = _commons.users_get__name_(_chat.user_1_id);
            this.user_2_ = _commons.users_get__name_(_chat.user_2_id);
            this.group_ = _commons.groups_get__name_(_chat.group_id);
        }
    }
    public class chat_dto
    {
        public string id { get; set; }
        public string user_1_ { get; set; }
        public string user_2_ { get; set; }
        public string group_ { get; set; }
        public chat_dto(chat _chat)
        {
            this.id = _chat.id;
            this.user_1_ = _commons.users_get__name_(_chat.user_1_id);
            this.user_2_ = _commons.users_get__name_(_chat.user_2_id);
            this.group_ = _commons.groups_get__name_(_chat.group_id);
        }
    }
    public class chat_dto_client_user
    {
        public string id { get; set; }
        public string title { get; set; }
        public string last_seen { get; set; }
        public string username { get; set; }
        public List<message_dto_client> messages { get; set; }
        
        public chat_dto_client_user(chat _chat, user _user)
        {
            // System.Console.WriteLine(_chat.user_1_id);
            // System.Console.WriteLine(_chat.user_2_id);
            string _user_target_id = _chat.user_1_id == _user.id ? _chat.user_2_id : _chat.user_1_id;
            // 
            this.id = _chat.id;
            this.title = _chats.get__title_(_chat.id, _user_target_id);
            this.last_seen = _commons.users_get__last_seen_(_user_target_id);
            this.username = _commons.users_get_(_user_target_id).username;
            this.messages = _messages.get_chat(this.id).OrderBy(x => x.dt_sent).Select(x => new message_dto_client(x, _user)).ToList();
        }
    }
    public class chat_dto_client_chat
    {
        public string id { get; set; }
        public string title { get; set; }
        public string avatar { get; set; }
        public string message { get; set; }
        public string username { get; set; }
        public int unread { get; set; } = 0;
        public string last_date { get; set; }
        public string dt { get; set; }
        public List<message_dto_client> messages { get; set; }
        
        public chat_dto_client_chat(chat _chat, user _user)
        {
            string _user_target_id = _chat.user_1_id == _user.id ? _chat.user_2_id : _chat.user_1_id;
            var _user_target = _commons.users_get_(_user_target_id);
            // 
            this.id = _chat.id;
            this.avatar = _user_target.avatar;
            this.username = _user_target.username;
            this.title = _chats.get__title_(_chat.id, _user_target_id);
            // 
            var message = _messages.get_chat(this.id).OrderByDescending(x => x.dt_sent).FirstOrDefault();
            if(message == null)
                return;
            this.message = message.text;
            this.unread = _messages.get_chat_unread(_user.id, _chat.id).Count();
            DateTime dt = stringify.ltodt(message.dt_sent);
            this.last_date = dt.ToString(entity.dt_format);
            if(dt.Date == DateTime.Now.Date)
                this.dt = dt.ToString(entity.time_format);
            else if(dt.Date == DateTime.Now.AddDays(-6).Date)
                this.dt = dt.ToString(entity.weekday_format);
            else
                this.dt = dt.ToString(entity.date_format);
        }
    }
}