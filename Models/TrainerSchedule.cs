using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nri_Webapplication_Backend.Models
{
    public class TrainerSchedule
    {
        public TrainerSchedule()
        {

        }

        public int eventScheduleId { get; set; }
        public int contentBusinessTypeId { get; set; }
        public int? statusId { get; set; }
        public int? langId { get; set; }
        public DateTime startDate { set; get; }
        public DateTime? endDate { set; get; }
        public String contentName { set; get; }
        public String room { set; get; }
        public String language { set; get; }
        public String businessTypeAbbreviation { set; get; }
        public String contentFormat { set; get; }
        public String location { set; get; }
        public String session { set; get; }
        public String company { set; get; }
        public String projectName { set; get; }
        public String bookBy { set; get; }
        public String remarks { set; get; }
        public String status { set; get; }
        public String flag { set; get; }


    }
}
