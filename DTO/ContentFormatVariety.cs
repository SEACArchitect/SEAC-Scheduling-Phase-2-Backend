using System;
using System.Collections.Generic;

namespace Nri_Webapplication_Backend.DTO
{
    public partial class ContentFormatVariety
    {
        public int ContentId { get; set; }
        public int ContentFormatId { get; set; }
        public int ContentFormatVarietyId { get; set; }

        public virtual ContentMaster Content { get; set; }
        public virtual ContentFormatMaster ContentFormat { get; set; }
    }
}
