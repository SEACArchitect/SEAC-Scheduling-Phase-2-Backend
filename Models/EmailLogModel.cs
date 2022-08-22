using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nri_Webapplication_Backend.Models
{
    public class EmailLogModel
    {
        public int Id { get; set; }
        public int? EventScheduleID { get; set; }
        public byte? IsEmailSentSuccess { get; set; }
        public DateTime? MailTimeStamp { get; set; }
        public byte? IsCancel { get; set; }
        public string UUID { get; set; }

        public int? TrainerID { get; set; }
   
    }

}
