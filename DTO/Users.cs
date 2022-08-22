using System;
using System.Collections.Generic;

namespace Nri_Webapplication_Backend.DTO
{
    public partial class Users
    {
        public Users()
        {
            UserLoginHistory = new HashSet<UserLoginHistory>();
        }

        public int UserId { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public byte IsActive { get; set; }
        public int UserRoleId { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public int? UpdatedBy { get; set; }
        public int UserRoleId2 { get; set; }

        public virtual UsersRole UserRole { get; set; }
        public virtual ICollection<UserLoginHistory> UserLoginHistory { get; set; }
    }
}
