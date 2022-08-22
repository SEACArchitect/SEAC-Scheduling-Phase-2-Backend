using System;
using System.Collections.Generic;

namespace Nri_Webapplication_Backend.DTO
{
    public partial class ContentBusinessType
    {
        public ContentBusinessType()
        {
            ContentMaster = new HashSet<ContentMaster>();
            EventSchedule = new HashSet<EventSchedule>();
            ModeratorSupportBusinessType = new HashSet<ModeratorSupportBusinessType>();
        }

        public int ContentBusinessTypeId { get; set; }
        public string ContentBusinessTypeName { get; set; }
        public string ContentBusinessTypeAbbreviate { get; set; }

        public virtual ICollection<ContentMaster> ContentMaster { get; set; }
        public virtual ICollection<EventSchedule> EventSchedule { get; set; }
        public virtual ICollection<ModeratorSupportBusinessType> ModeratorSupportBusinessType { get; set; }
    }
}
