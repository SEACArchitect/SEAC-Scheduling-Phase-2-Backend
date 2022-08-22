using System;
using System.Collections.Generic;

namespace Nri_Webapplication_Backend.DTO
{
    public partial class ContentFormatMaster
    {
        public ContentFormatMaster()
        {
            ContentFormatVariety = new HashSet<ContentFormatVariety>();
            EventRoomMaster = new HashSet<EventRoomMaster>();
            EventSchedule = new HashSet<EventSchedule>();
        }

        public int ContentFormatId { get; set; }
        public string ContentFormatName { get; set; }

        public virtual ICollection<ContentFormatVariety> ContentFormatVariety { get; set; }
        public virtual ICollection<EventRoomMaster> EventRoomMaster { get; set; }
        public virtual ICollection<EventSchedule> EventSchedule { get; set; }
    }
}
