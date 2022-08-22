using System;
using System.Collections.Generic;

namespace Nri_Webapplication_Backend.DTO
{
    public partial class Language
    {
        public Language()
        {
            EventSchedule = new HashSet<EventSchedule>();
        }

        public int LanguageId { get; set; }
        public string LanguageName { get; set; }

        public virtual ICollection<EventSchedule> EventSchedule { get; set; }
    }
}
