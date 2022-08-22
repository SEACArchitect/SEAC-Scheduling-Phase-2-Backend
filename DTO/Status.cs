using System;
using System.Collections.Generic;

namespace Nri_Webapplication_Backend.DTO
{
    public partial class Status
    {
        public Status()
        {
            EventSchedule = new HashSet<EventSchedule>();
        }

        public int StatusId { get; set; }
        public string Status1 { get; set; }

        public virtual ICollection<EventSchedule> EventSchedule { get; set; }
    }
}
