using System;
using System.Collections.Generic;

namespace Nri_Webapplication_Backend.DTO
{
    public partial class ModeratorSkill
    {
        public int ModeratorId { get; set; }
        public int SkillId { get; set; }
        public byte IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public int? UpdatedBy { get; set; }
        public int Id { get; set; }

        public virtual TrainerMaster Moderator { get; set; }
        public virtual Skill Skill { get; set; }
    }
}
