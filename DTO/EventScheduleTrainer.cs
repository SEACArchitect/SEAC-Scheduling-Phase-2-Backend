using System;
using System.Collections.Generic;

namespace Nri_Webapplication_Backend.DTO
{
    public partial class EventScheduleTrainer
    {
        public int EventScheduleId { get; set; }
        public int TrainerId { get; set; }
        public byte IsActive { get; set; }
        public byte? IsTrainerReply { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public int? UpdatedBy { get; set; }
        public int Id { get; set; }

        public virtual EventSchedule EventSchedule { get; set; }
        public virtual TrainerMaster Trainer { get; set; }
    }
}
