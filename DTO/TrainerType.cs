using System;
using System.Collections.Generic;

namespace Nri_Webapplication_Backend.DTO
{
    public partial class TrainerType
    {
        public TrainerType()
        {
            TrainerMaster = new HashSet<TrainerMaster>();
        }

        public int TrainerTypeId { get; set; }
        public string TypeName { get; set; }

        public virtual ICollection<TrainerMaster> TrainerMaster { get; set; }
    }
}
