using System;
using System.Collections.Generic;

namespace Nri_Webapplication_Backend.DTO
{
    public partial class EmailLog
    {
        public int Id { get; set; }
        public int? EventScheduleId { get; set; }
        public byte? IsEmailSentSuccess { get; set; }
        public DateTime? MailTimeStamp { get; set; }
        public byte? IsCancel { get; set; }
        public string Uuid { get; set; }
        public int? TrainerId { get; set; }
    }
}
