using System;
using System.Collections.Generic;

namespace Nri_Webapplication_Backend.DTO
{
    public partial class CertifiedTrainer
    {
        public int CertifiedTrainerId { get; set; }
        public int ContentId { get; set; }
        public int TrainerId { get; set; }
        public byte IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public int? UpdatedBy { get; set; }
    }
}
