using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using mercury.model;
using Microsoft.EntityFrameworkCore;
using mercury.business;
using mercury.controller;
using Newtonsoft.Json;

namespace mercury.data
{
    public static class _chats
    {
        #region get
        public static IEnumerable<chat> get()
        {
            return dbc_mercury.chats;
        }
        public static IEnumerable<chat> get_search(string s, IEnumerable<chat> query = null)
        {
            if (query == null)
                query = dbc_mercury.chats;
            return query.Where(x => get__search(x, s));
        }
        public static IEnumerable<chat> get(int skip, int take, ref int count, bool asc = false, string sort_by = "id", string d_p = null, string s = null, long? dt_from = null, long? dt_to = null)
        {
            var propertyInfo = typeof(chat).GetProperty(sort_by);
            if (propertyInfo == null)
                propertyInfo = typeof(chat).GetProperty("id");
            var query = get();
            if (!string.IsNullOrEmpty(d_p))
                query = query.Where(x => x.id == d_p);
            if (!string.IsNullOrEmpty(s) && d_p == null)
                query = query.Where(x => get__search(x, s));
            //
            count = query.Count();
            if (asc)
                query = query.OrderBy(x => propertyInfo.GetValue(x, null));
            else
                query = query.OrderByDescending(x => propertyInfo.GetValue(x, null));
            return query.Skip(skip).Take(take);
        }
        public static IEnumerable<chat> get_chat(string chat_id)
        {
            return dbc_mercury.chats.Where(x => x.id == chat_id);
        }
        public static IEnumerable<chat> get_user(string user_id)
        {
            return dbc_mercury.chats.Where(x => x.user_1_id == user_id || x.user_2_id == user_id || _commons.group_users_get_(x.group_id, user_id) != null);
        }
        public static string get_target_id(string id, string user_id)
        {
            var _chat = dbc_mercury.chats.FirstOrDefault(x => x.id == id);
            if (_chat == null)
                return null;
            return _chat.user_1_id == user_id ? _chat.user_2_id : _chat.user_1_id;
        }
        public static chat get_group(string group_id)
        {
            return dbc_mercury.chats.FirstOrDefault(x => x.group_id == group_id);
        }
        #endregion
        #region get_
        public static chat get_(string id)
        {
            chat _chat = null;
            _chat = dbc_mercury.chats.FirstOrDefault(x => x.id == id);
            return _chat;
        }
        public static chat get_(string user_1_id, string user_2_id)
        {
            return dbc_mercury.chats.FirstOrDefault(x =>
                (x.user_1_id == user_1_id && x.user_2_id == user_2_id)
                || (x.user_2_id == user_1_id && x.user_1_id == user_2_id)
                );
        }
        public static string get__title_(string id, string _user_target_id)
        {
            chat _chat = get_(id);
            if (_chat == null)
                return message_sys.unknow;
            if (string.IsNullOrEmpty(_chat.group_id))
            {
                return _commons.users_get_(_user_target_id).get_name();
            }
            return _commons.groups_get__name_(_chat.group_id);
        }
        public static chat get__group(string group_id)
        {
            return dbc_mercury.chats.FirstOrDefault(x => x.group_id == group_id);
        }
        public static bool get__search(string id, string s)
        {
            var item = get_(id);
            if (item == null)
                return false;
            return get__search(item, s);
        }
        public static bool get__search(chat item, string s)
        {
            return _chats.get__search(item.user_1_id, s) || _chats.get__search(item.user_2_id, s) ||
                 _commons.group_users_get_group(item.group_id).Any(y => _chats.get__search(y.user_id, s)) || _messages.get__search(item.id, s, false);
        }
        public static bool is_user_in(string id, string user_id)
        {
            chat _chat = get_(id);
            if (_chat == null)
                return false;
            return _chat.user_1_id == user_id || _chat.user_2_id == user_id || _commons.group_users_get_(_chat.group_id, user_id) != null;
        }
        #endregion
        #region set
        public static bool is_allow_message_send(user _user, string id)
        {
            chat _chat = get_(id);
            if (_chat == null)
                return false;
            if (_chat.user_1_id != _user.id && _chat.user_2_id != _user.id)
                return false;
            group_user _group_user = _commons.group_users_get_(_chat.group_id, _user.id);
            if (_group_user == null)
                return false;
            //check permission
            //check if blocked or restricted
            return true;
        }
        public static bool clear(user _user, string id, ref string str)
        {
            chat _chat = get_(id);
            if (_chat == null)
            {
                str = message_sys.null_refrence;
                return false;
            }
            if (!is_user_in(_chat.id, _user.id))
            {
                str = message_sys.not_your_item;
                return false;
            }
            if (_chat.group_id != null)
            {
                //check permission
                str = message_sys.permission_denied;
                return false;
            }
            var messages = _messages.get_chat(_chat.id).ToArray();
            foreach (message _message in messages)
            {
                _messages.delete(_message.id);
            }
            return true;
        }
        public static chat add(string user_1_id, string user_2_id)
        {
            if (_chats.get_(user_1_id, user_2_id) != null)
                return null;
            chat item = new chat();
            item.id = entity.id_new;
            item.user_1_id = user_1_id;
            item.user_2_id = user_2_id;
            item.group_id = null;
            ctrl_db.insert_chat(item);
            dbc_mercury.chats.Add(item);
            return item;
        }
        public static chat add(string group_id)
        {
            if (_chats.get__group(group_id) == null)
                return null;
            chat item = new chat();
            item.id = entity.id_new;
            item.user_1_id = null;
            item.user_2_id = null;
            item.group_id = group_id;
            ctrl_db.insert_chat(item);
            dbc_mercury.chats.Add(item);
            return item;
        }
        public static void set_read(string user_id, string id)
        {
            var item = get_(id);
            if(item == null)
                return;
            _messages.set_read(user_id, id);
        }
        #endregion
        #region delete
        public static bool delete(user _user, string id, ref string str)
        {
            chat item = get_(id);
            if (item == null)
            {
                str = message_sys.null_refrence;
                return false;
            }
            if (!is_user_in(id, _user.id))
            {
                str = message_sys.not_your_item;
                return false;
            }
            if (item.group_id != null)
            {
                //check permission
                str = message_sys.permission_denied;
                return false;
            }
            dbc_mercury.chats.Remove(item);
            ctrl_db.delete_chat(item);
            return true;
        }
        public static bool delete(string id)
        {
            chat item = get_(id);
            if (item == null)
                return false;
            dbc_mercury.chats.Remove(item);
            ctrl_db.delete_chat(item);
            return true;
        }
        #endregion
    }
}