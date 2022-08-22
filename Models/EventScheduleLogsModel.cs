using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nri_Webapplication_Backend.Models
{
    public class EventScheduleLogsModel
    {
        public string date { get; set; }
        public string time { get; set; }
        public int eventId { get; set; }
        public string eventName { get; set; }
         public string eventDate { get; set; }
        public string businessType { get; set; }
        public string category { get; set; }
        public string action { get; set; }
        public string field { get; set; }
        public string fromValue { get; set; }
        public string toValue { get; set; }       
        public string changedBy { get; set; }
        public string remark { get; set; } 
        public string company { get; set; } 
        public string projectId { get; set; } 
        public string bookBy { get; set; } 

    }
}
