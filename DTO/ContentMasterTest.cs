using System;
using System.Collections.Generic;

namespace Nri_Webapplication_Backend.DTO
{
    public partial class ContentMasterTest
    {
        public int ContentId { get; set; }
        public string ContentCode { get; set; }
        public string ContentName { get; set; }
        public string DisplayName { get; set; }
        public string OriginalContentName { get; set; }
        public decimal Duration { get; set; }
        public byte IsActive { get; set; }
        public int PaxMax { get; set; }
        public byte? IsInternal { get; set; }
        public byte? IsPrivillege { get; set; }
        public int LearningTypeId { get; set; }
        public int ContentBusinessTypeId { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public int? UpdatedBy { get; set; }

        public virtual ContentBusinessType ContentBusinessType { get; set; }
        public virtual LearningTypeMaster LearningType { get; set; }
    }
}
