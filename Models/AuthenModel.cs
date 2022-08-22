using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nri_Webapplication_Backend.Models
{
    public enum Roles
    {
        admin = 1,
        adminReadOnly = 2,
        viewer = 3
    }
    public class AuthenModel
    {
        public string username { get; set; }
        public string password { get; set; }
    }
}
