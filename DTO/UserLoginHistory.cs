using System;
using System.Collections.Generic;

namespace Nri_Webapplication_Backend.DTO
{
    public partial class UserLoginHistory
    {
        public int UserLogInHistory1 { get; set; }
        public int UserId { get; set; }
        public DateTime LogInDateTime { get; set; }

        public virtual Users User { get; set; }
    }
}
