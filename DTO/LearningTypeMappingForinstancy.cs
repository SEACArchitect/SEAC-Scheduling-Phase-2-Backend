using System;
using System.Collections.Generic;

namespace Nri_Webapplication_Backend.DTO
{
    public partial class LearningTypeMappingForinstancy
    {
        public int LearningTypeId { get; set; }
        public int ContentFormatId { get; set; }
        public int? Id { get; set; }
        public string Value { get; set; }
        public string Description { get; set; }

        public virtual ContentFormatMaster ContentFormat { get; set; }
        public virtual LearningTypeMaster LearningType { get; set; }
    }
}
