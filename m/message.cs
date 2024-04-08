using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using mercury.business;
using mercury.data;
using Newtonsoft.Json;

namespace mercury.model
{
    [Serializable]
    public class message_dto_min
    {
        public string id { get; set; }
        public string user_ { set; get; }
        public string chat_ { get; set; }
        public string text { get; set; }
        public string text_edited { get; set; }
        public int type { get; set; } = 0;
        public string dt_sent { set; get; }
        public string dt_update { set; get; }
        public message_dto_min(message _message, user _user)
        {
            this.id = _message.id;
            this.user_ = _commons.users_get__name_(_message.user_id);
            this.text = _message.text.Substring(0, Math.Min(50, _message.text_edited.Length));
            this.text_edited = _message.text_edited.Substring(0, Math.Min(50, _message.text_edited.Length));
            this.type = _message.type;
            this.chat_ = _chats.get__title_(_message.chat_id, _user.id);
            this.dt_sent = stringify.ltodt(_message.dt_sent).ToString(entity.dt_format);
            this.dt_update = stringify.ltodt(_message.dt_update).ToString(entity.dt_format);
        }
    }

    [Serializable]
    public class message_dto
    {
        public string id { get; set; }
        public string user_ { set; get; }
        public string chat_ { get; set; }
        public string text { get; set; }
        public string text_edited { get; set; }
        public int type { get; set; } = 0;
        public string attachment_ { get; set; }
        public string forwarded_from { get; set; }
        public string replied_to { get; set; }
        public string dt_sent { set; get; }
        public string dt_update { set; get; }
        public message_dto(message _message, user _user)
        {
            this.id = _message.id;
            this.user_ = _commons.users_get__name_(_message.user_id);
            this.text = _message.text;
            this.text_edited = _message.text_edited;
            this.type = _message.type;
            this.attachment_ = entity.url_attachment_org(_message.attachment_id);
            this.chat_ = _chats.get__title_(_message.chat_id, _user.id);
            this.forwarded_from = _commons.users_get__name_(_message.forward_id);
            this.replied_to = _commons.users_get__name_(_message.reply_id);
            this.dt_sent = stringify.ltodt(_message.dt_sent).ToString(entity.dt_format);
            this.dt_update = stringify.ltodt(_message.dt_update).ToString(entity.dt_format);
        }
    }
    public class message_dto_client
    {
        public string id { get; set; }
        public string chat_id { get; set; }
        public string user_ { set; get; }
        public string text { get; set; }
        public string text_edited { get; set; }
        public int type { get; set; } = 0;
        public string attachment_ { get; set; }
        public string forwarded_from { get; set; }
        public string replied_to { get; set; }
        public string dt { set; get; }
        public string t { set; get; }
        public string rtl { set; get; } = "false";
        public string end { set; get; } = "false";
        public message_dto_client(message _message, user _user)
        {
            this.id = _message.id;
            this.chat_id = _message.chat_id;
            this.user_ = _commons.users_get__name_(_message.user_id);
            this.text = _message.text;
            this.text_edited = _message.text_edited;
            this.type = _message.type;
            this.attachment_ = entity.url_attachment_org(_message.attachment_id);
            this.forwarded_from = _commons.users_get__name_(_message.forward_id);
            this.replied_to = _commons.users_get__name_(_message.reply_id);
            this.dt = stringify.ltodt(_message.dt_sent).ToString(entity.dt_format);
            this.t = stringify.ltodt(_message.dt_sent).ToString(entity.time_format);
            this.rtl = "false"; //check
            this.end = (_message.user_id != _user.id).ToString().ToLower();
        }
    }
}