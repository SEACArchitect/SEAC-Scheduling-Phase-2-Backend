using System;
using System.Collections.Generic;

namespace Nri_Webapplication_Backend.DTO
{
    public partial class Skill
    {
        public Skill()
        {
            ModeratorSkill = new HashSet<ModeratorSkill>();
        }

        public int SkillId { get; set; }
        public string SkillName { get; set; }

        public virtual ICollection<ModeratorSkill> ModeratorSkill { get; set; }
    }
}
