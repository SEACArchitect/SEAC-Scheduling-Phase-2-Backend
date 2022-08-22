using Microsoft.AspNetCore.Diagnostics;
using Microsoft.VisualStudio.Web.CodeGeneration.Contracts.Messaging;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;

namespace Nri_Webapplication_Backend.Models
{
    public class RespondModel
    {
        public int? total { get; set; }//Total in database
        public int? count { get; set; }//Count request 
        public string message { get; set; }//Message normail
        public string error { get; set; } //Message error 
        public dynamic? data { get; set; } //Result request 
    }
}
