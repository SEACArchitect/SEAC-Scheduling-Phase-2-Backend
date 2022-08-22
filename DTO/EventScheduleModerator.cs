using System;
using System.Collections.Generic;

namespace Nri_Webapplication_Backend.DTO
{
    public partial class EventScheduleModerator
    {
        public int EventScheduleId { get; set; }
        public int ModeratorId { get; set; }
        public byte IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CreatedBy { get; set; }
        public int Id { get; set; }

        public virtual EventSchedule EventSchedule { get; set; }
        public virtual TrainerMaster Moderator { get; set; }
    }
}
