using System;
using System.Collections.Generic;

namespace Nri_Webapplication_Backend.DTO
{
    public partial class UsersRole
    {
        public UsersRole()
        {
            Users = new HashSet<Users>();
        }

        public int UserRoleId { get; set; }
        public string UserRoleName { get; set; }

        public virtual ICollection<Users> Users { get; set; }
    }
}
