using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nri_Webapplication_Backend.Models
{
    public class ModeratorSummaryModel
    {
        public string startTime { get; set; }
        public string endTime { get; set; }
        public string moderatorType { get; set; }
        public string contractType { get; set; }
        public string moderatorName { get; set; }
        public string nickNameEn { get; set; }
        public string bookingType { get; set; }   
        public string contentName { get; set; }       
        public string trainerName { get; set; }  
        public double noOfHours { get; set; }
        public double noOfSession { get; set; }
        public int year { get; set; }
        public string month { get; set; }
    }
}
