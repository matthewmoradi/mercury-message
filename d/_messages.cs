using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using mercury.model;
using mercury.controller;
using Microsoft.EntityFrameworkCore;
using mercury.business;
using Newtonsoft.Json;

namespace mercury.data
{
    public static class _messages
    {
        #region get
        public static IEnumerable<message> get()
        {
            return dbc_mercury.messages;
        }
        public static IEnumerable<message> get_search(string s, IEnumerable<message> query = null, bool deep = true)
        {
            if (query == null)
                query = dbc_mercury.messages;
            return query.Where(x => get__search(x, s));
        }
        public static IEnumerable<message> get(int skip, int take, ref int count, bool asc = false, string sort_by = "id", string d_p = null, string s = null, long? dt_from = null, long? dt_to = null)
        {
            var propertyInfo = typeof(message).GetProperty(sort_by);
            if (propertyInfo == null)
                propertyInfo = typeof(message).GetProperty("id");
            var query = get();
            if (!string.IsNullOrEmpty(d_p))
                query = query.Where(x => x.id == d_p);
            if (!string.IsNullOrEmpty(s) && d_p == null)
                query = query.Where(x => _commons.users_get_(x.user_id).username.Contains(s) || x.text.Contains(s));
            if (dt_from != null)
                query = query.Where(x => x.dt_sent > dt_from.Value);
            if (dt_to != null)
                query = query.Where(x => x.dt_sent < dt_to.Value);
            // 
            count = query.Count();
            if (asc)
                query = query.OrderBy(x => propertyInfo.GetValue(x, null));
            else
                query = query.OrderByDescending(x => propertyInfo.GetValue(x, null));
            return query.Skip(skip).Take(take);
        }
        public static IEnumerable<message> get(string user_id, string chat_id, int skip, int take, long? dt_from = null, long? dt_to = null)
        {
            var query = dbc_mercury.messages.Where(x => x.user_id == user_id && x.chat_id == chat_id);
            if (dt_from != null)
                query = query.Where(x => x.dt_sent >= dt_from);
            if (dt_to != null)
                query = query.Where(x => x.dt_sent <= dt_to);
            return query.Skip(skip).Take(take);
        }
        public static IEnumerable<message> search(string user_id, string s, string chat_id)
        {
            var query = dbc_mercury.messages.Where(x => x.user_id == user_id);
            if (!string.IsNullOrEmpty(chat_id))
                query = query.Where(x => x.chat_id == chat_id);
            query = get_search(s, query, false);
            return query;
        }
        public static IEnumerable<message> get_user(string user_id)
        {
            return dbc_mercury.messages.Where(x => x.id == user_id);
        }
        public static IEnumerable<message> get_chat(string chat_id)
        {
            return dbc_mercury.messages.Where(x => x.chat_id == chat_id);
        }
        public static IEnumerable<message> get_chat_unread(string user_id, string chat_id)
        {
            return dbc_mercury.messages.Where(x => x.chat_id == chat_id && !x.read && x.user_id != user_id);
        }
        public static IEnumerable<message> get_chat(string chat_id, int type)
        {
            return dbc_mercury.messages.Where(x => x.chat_id == chat_id && x.type == type);
        }
        #endregion
        #region get_

        public static message get_(string id)
        {
            message _message = null;
            _message = dbc_mercury.messages.FirstOrDefault(x => x.id == id);
            return _message;
        }
        public static bool get__search(string id, string s, bool deep = true)
        {
            var item = get_(id);
            if (item == null)
                return false;
            return get__search(item, s, deep);
        }
        public static bool get__search(message item, string s, bool deep = true)
        {
            bool res = item.text.Contains(s) || item.text_edited.Contains(s);
            if (deep)
                res = res || _commons.users_get__search(item.user_id, s) || _commons.users_get__search(item.forward_id, s) || _commons.users_get__search(item.reply_id, s);
            return res;
        }
        #endregion
        #region set
        public static bool is_valid_text(string text)
        {
            return text.Length > 0 && text.Length < entity.max_message_len;
        }
        public static message add(user _user, string chat_id, string text, string reply_id, string attachment, int type, ref string str)
        {
            if (!is_valid_text(text))
            {
                str = message_sys.message_text_not_valid;
                return null;
            }
            chat _chat = _chats.get_(chat_id);
            if (_chat == null || !_chats.is_user_in(chat_id, _user.id))
            {
                str = message_sys.not_allowed_send_in_this_chat;
                return null;
            }
            //check permissions and restrictions
            // attachment _attachment = _attachments.add("", attachment, ""); //add
            if (!string.IsNullOrEmpty(_chat.group_id))
            {

            }
            message item = new message();
            item.id = entity.id_new;
            item.user_id = _user.id;
            item.chat_id = chat_id;
            item.text = text;
            item.forward_id = null;
            item.reply_id = reply_id;
            // item.attachment_id = _attachment.id;
            item.type = type;
            item.read = false;
            item.dt_update = item.dt_sent = stringify.dttol(DateTime.Now);
            ctrl_db.insert_message(item);
            dbc_mercury.messages.Add(item);
            return item;
        }
        public static message forward(user _user, message _message_, string chat_id)
        {
            message item = new message();
            item.id = entity.id_new;
            item.user_id = _user.id;
            item.chat_id = chat_id;
            item.text = _message_.text;
            item.forward_id = _message_.id;
            item.reply_id = null;
            item.attachment_id = _message_.attachment_id;
            item.type = _message_.type;
            item.read = false;
            item.dt_update = item.dt_sent = stringify.dttol(DateTime.Now);
            ctrl_db.insert_message(item);
            dbc_mercury.messages.Add(item);
            return item;
        }
        public static bool edit(user _user, string id, string text_edited, ref string str)
        {
            message item = get_(id);
            if (item == null)
            {
                str = message_sys.null_refrence;
                return false;
            }
            if (item.user_id != _user.id)
            {
                str = message_sys.not_your_item;
                return false;
            }
            if (!is_valid_text(text_edited))
            {
                str = message_sys.message_text_not_valid;
                return false;
            }
            item.text_edited = text_edited;
            item.dt_update = stringify.dttol(DateTime.Now);
            ctrl_db.update_message(item);
            return true;
        }
        public static void set_read(string user_id, string chat_id)
        {
            var items = get_chat_unread(user_id, chat_id);
            if (!items.Any())
                return;
            foreach (var item in items)
            {
                item.read = true;
                ctrl_db.update_message(item);
            }
        }
        public static bool forward(user _user, string id, string[] chat_ids, ref string str)
        {
            message item = get_(id);
            if (item == null)
            {
                str = message_sys.null_refrence;
                return false;
            }
            bool not_allowed = chat_ids.Any(x => _chats.get_(x) == null || !_chats.is_user_in(x, _user.id));
            if (!not_allowed)
            {
                str = message_sys.not_allowed_send_in_this_chat;
                return false;
            }
            foreach (string chat_id in chat_ids)
            {
                forward(_user, item, chat_id);
            }
            return true;
        }
        #endregion
        #region delete
        public static bool delete(user _user, string id, ref string str)
        {
            message item = get_(id);
            if (item == null)
            {
                str = message_sys.null_refrence;
                return false;
            }
            if (item.user_id != _user.id)
            {
                str = message_sys.not_your_item;
                return false;
            }
            dbc_mercury.messages.Remove(item);
            ctrl_db.delete_message(item);
            return true;
        }
        public static bool delete(string id)
        {
            message item = get_(id);
            if (item == null)
                return false;
            dbc_mercury.messages.Remove(item);
            ctrl_db.delete_message(item);
            return true;
        }
        #endregion
    }
}