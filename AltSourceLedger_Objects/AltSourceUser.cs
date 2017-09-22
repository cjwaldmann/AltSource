using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AltSourceLedger_Objects
{
    public class AltSourceUser
    {
        public string username { get; set; }
        public string password { get; set; }
        

        public static AltSourceUser CreateUser(string uname, string pwd)
        {
            AltSourceUser user = new AltSourceUser();
            user.username = uname;
            user.password = pwd;

            return user;
        }
    }
}
