using System;
using System.Collections.Generic;

namespace Nri_Webapplication_Backend.DTO
{
    public partial class ScheduleAcceptedLog
    {
        public int ScheduleAcceptedId { get; set; }
        public int EventScheduleId { get; set; }
        public int TrainerId { get; set; }
        public byte? IsAccepted { get; set; }
        public string Uuid { get; set; }
    }
}
