using System;
using System.Collections.Generic;

namespace Nri_Webapplication_Backend.DTO
{
    public partial class TrainerContractType
    {
        public TrainerContractType()
        {
            TrainerMaster = new HashSet<TrainerMaster>();
        }

        public int TraninerContractTypeId { get; set; }
        public string TraninerContractTypeName { get; set; }

        public virtual ICollection<TrainerMaster> TrainerMaster { get; set; }
    }
}
