using System;
using System.Collections.Generic;

namespace Nri_Webapplication_Backend.DTO
{
    public partial class EventCategory
    {
        public EventCategory()
        {
            EventSchedule = new HashSet<EventSchedule>();
        }

        public int EventCategoryId { get; set; }
        public string EventCategoryName { get; set; }

        public virtual ICollection<EventSchedule> EventSchedule { get; set; }
    }
}
