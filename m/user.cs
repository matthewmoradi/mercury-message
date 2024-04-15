using System;
using System.Linq;
using mercury.business;
using mercury.data;
using Newtonsoft.Json;

namespace mercury.model
{
    [Serializable]
    public class user_min : user
    {
        public string name { get; set; }
        public string dt_register { get; set; }
        public string dt_last_login { get; set; } = null;
        public string dt_login_fail { get; set; } = null;
        public int status { get; set; }
        public string socket_id { get; set; }
        public string k { get; set; }
        public bool valid { get; set; }
        public user_min(user _user, string socket_id)
        {
            this.id = _user.id;
            this.username = _user.username;
            this.name = _user.get_name();
            this.dt_register = stringify.ltodt(_user.dt_register).ToString(entity.dt_format);
            this.dt_last_login = stringify.ltodt(_user.dt_last_login).ToString(entity.dt_format);
            this.dt_login_fail = stringify.ltodt(_user.dt_login_fail).ToString(entity.dt_format);
            this.phone = _user.phone;
            this.email = _user.email;
            this.status = _user.status;
            this.socket_id = socket_id;
            this.valid = stringify.hash(_user) == _user.hash;
        }
    }
}