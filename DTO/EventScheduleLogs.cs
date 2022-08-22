using System;
using System.Collections.Generic;

namespace Nri_Webapplication_Backend.DTO
{
    public partial class EventScheduleLogs
    {
        public int EventScheduleLogsId { get; set; }
        public int EventScheduleId { get; set; }
        public int UserId { get; set; }
        public string Action { get; set; }
        public int ContentId { get; set; }
        public string Field { get; set; }
        public string FromValue { get; set; }
        public string ToValue { get; set; }
        public DateTime LoggedDateTime { get; set; }
    }
}
