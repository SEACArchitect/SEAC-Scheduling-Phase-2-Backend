using System;
using System.Collections.Generic;

namespace Nri_Webapplication_Backend.DTO
{
    public partial class LearningTypeMaster
    {
        public LearningTypeMaster()
        {
            ContentMaster = new HashSet<ContentMaster>();
            EventSchedule = new HashSet<EventSchedule>();
        }

        public int LearningTypeId { get; set; }
        public string LearningTypeName { get; set; }

        public virtual ICollection<ContentMaster> ContentMaster { get; set; }
        public virtual ICollection<EventSchedule> EventSchedule { get; set; }
    }
}
