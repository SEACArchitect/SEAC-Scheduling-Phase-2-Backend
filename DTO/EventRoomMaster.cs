using System;
using System.Collections.Generic;

namespace Nri_Webapplication_Backend.DTO
{
    public partial class EventRoomMaster
    {
        public EventRoomMaster()
        {
            EventSchedule = new HashSet<EventSchedule>();
        }

        public int EventRoomId { get; set; }
        public string EventRoomName { get; set; }
        public int EventRoomTypeId { get; set; }
        public string InstancyLocationId { get; set; }
        public string InstancyDisplayName { get; set; }
        public byte IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public int? UpdatedBy { get; set; }
        public int ContentFormatId { get; set; }

        public virtual ContentFormatMaster ContentFormat { get; set; }
        public virtual EventRoomType EventRoomType { get; set; }
        public virtual ICollection<EventSchedule> EventSchedule { get; set; }
    }
}
