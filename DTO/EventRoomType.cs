using System;
using System.Collections.Generic;

namespace Nri_Webapplication_Backend.DTO
{
    public partial class EventRoomType
    {
        public EventRoomType()
        {
            EventRoomMaster = new HashSet<EventRoomMaster>();
            ModeratorSupportRoomType = new HashSet<ModeratorSupportRoomType>();
        }

        public int EventRoomTypeId { get; set; }
        public string EventRoomTypeName { get; set; }

        public virtual ICollection<EventRoomMaster> EventRoomMaster { get; set; }
        public virtual ICollection<ModeratorSupportRoomType> ModeratorSupportRoomType { get; set; }
    }
}
