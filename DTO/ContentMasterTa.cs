using System;
using System.Collections.Generic;

namespace Nri_Webapplication_Backend.DTO
{
    public partial class ContentMasterTa
    {
        public string ContentCode { get; set; }
        public string ContentName { get; set; }
        public string DisplayName { get; set; }
        public string OriginalContentName { get; set; }
        public int? Duration { get; set; }
        public int? IsActive { get; set; }
        public int? PaxMax { get; set; }
        public int? IsInternal { get; set; }
        public int? IsPrivillege { get; set; }
        public int? LearningTypeId { get; set; }
        public int? ContentBusinessTypeId { get; set; }
        public string CreatedDate { get; set; }
        public int? CreatedBy { get; set; }
    }
}
