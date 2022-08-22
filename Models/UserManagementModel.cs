
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nri_Webapplication_Backend.Models
{
    public class UserManagementModel : BaseModel
    {
        public int userID { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public byte isActive { get; set; }
        public int userRoleID { get; set; }
        public string userRoleName { get; set; }
        public string lastLogin { get; set; }
        public int userRoleID2 { get; set; }
        public string userRoleName2 { get; set; }
    }
}
