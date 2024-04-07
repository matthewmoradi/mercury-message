using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using mercury.business;
using Newtonsoft.Json;
using System.IO;
using mercury.controller;

namespace mercury.model
{
    public static class dbc_mercury
    {
        public static List<user> users { get; set; } = new List<user>();
        public static List<group> groups { get; set; } = new List<group>();
        public static List<group_user> group_users { get; set; } = new List<group_user>();
        public static List<chat> chats { get; set; } = new List<chat>();
        public static List<message> messages { get; set; } = new List<message>();
        public static void init()
        {
            users = ctrl_db.get_users();
            groups = ctrl_db.get_groups();
            group_users = ctrl_db.get_group_users();
            chats = ctrl_db.get_chats();
            messages = ctrl_db.get_messages();
        }
    }
}