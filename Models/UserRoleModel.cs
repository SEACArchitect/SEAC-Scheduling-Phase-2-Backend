﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nri_Webapplication_Backend.Models
{
    public class UserRoleModel
    {
        public int id { get; set; }
        public string value { get; set; }
    }
    public class ModManagerModel
    {
        public string Email { get; set; }
        public int RoleId2 { get; set; }
    }
}
