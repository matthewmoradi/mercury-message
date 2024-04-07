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
    public static class _commons
    {
        public static user users_get_(string id)
        {
            return dbc_mercury.users.FirstOrDefault(x => x.id == id);
        }
        public static user users_get__username(string username)
        {
            return dbc_mercury.users.FirstOrDefault(x => x.username == username);
        }
        public static bool users_get__search(string id, string s)
        {
            var item = users_get_(id);
            if (item == null)
                return false;
            return users_get__search(item, s);
        }
        public static bool users_get__search(user item, string s)
        {
            return item.username.Contains(s) || item.name_first.Contains(s) || item.name_last.Contains(s) || item.email.Contains(s) || item.phone.Contains(s);
        }
        public static string users_get__name_(string id)
        {
            var item = users_get_(id);
            if (item != null)
                return item.get_name();
            return message_sys.unknow;
        }
        public static string users_get__last_seen_(string id)
        {
            var item = users_get_(id);
            if (item != null)
                return "last seen recently";
            return message_sys.unknow;
        }
        // 
        public static group groups_get_(string id)
        {
            return dbc_mercury.groups.FirstOrDefault(x => x.id == id);
        }
        public static string groups_get__name_(string id)
        {
            group _group = groups_get_(id);
            if (_group == null)
                return message_sys.unknow;
            return _group.name;
        }
        // 
        public static group_user group_users_get_(string id)
        {
            return dbc_mercury.group_users.FirstOrDefault(x => x.id == id);
        }
        public static group_user group_users_get_(string group_id, string user_id)
        {
            return dbc_mercury.group_users.FirstOrDefault(x => x.group_id == group_id && x.user_id == user_id);
        }
        public static IEnumerable<group_user> group_users_get_group(string group_id)
        {
            return dbc_mercury.group_users.Where(x => x.group_id == group_id);
        }
    }
}