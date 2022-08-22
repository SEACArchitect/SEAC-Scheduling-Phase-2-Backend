using Microsoft.AspNetCore.Mvc;
using Nri_Webapplication_Backend.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nri_Webapplication_Backend.Helpers
{
    public interface IRespondHelper
    {
        public RespondModel Respond(int? total,int? count,string message,string error,dynamic data);
    }
    public class RespondHelper : IRespondHelper
    {
        public RespondHelper()
        {
            
        }
    
        public RespondModel Respond(int? total, int? count, string message, string error, dynamic data)
        {
            return new RespondModel { total = total , count = count , message = message ,error = error , data = data };
        }
    }
}
