using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nri_Webapplication_Backend.Models
{
    public class BaseModel
    {
        public DateTime createdDate { get; set; }
        public DateTime? updatedDate { get; set; }
        public int createdBy { get; set; }
        public int? updatedBy { get; set; }
    }
}
