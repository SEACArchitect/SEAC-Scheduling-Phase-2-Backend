using Nri_Webapplication_Backend.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nri_Webapplication_Backend.Models
{
    public class AutoMailAppoinmentModel
    {
        public EventSchedule EventSchedule { get; set; }
        public DTO.ContentBusinessType ContentBusinessType { get; set; }

        public DTO.ContentMaster ContentMaster { get; set; }

        public EventRoomMaster EventRoomMaster { get; set; }
        public LearningTypeMaster  LearningTypeMaster { get; set; }
        public EventScheduleTrainer EventScheduleTrainer { get; set; }
        public  TrainerMaster TrainerMaster { get; set; }

        public Status Status { get; set; }
        public EmailLog emailLog { get; set; }
    }
}
