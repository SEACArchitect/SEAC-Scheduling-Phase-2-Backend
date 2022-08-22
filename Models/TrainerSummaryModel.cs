using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nri_Webapplication_Backend.Models
{
    public class TrainerSummaryModel
    {
        public string startTime { get; set; }
        public string endTime { get; set; }
        public string trainerType { get; set; }        
        public string contractType { get; set; }
        public string trainerName { get; set; }
        public string bookingType { get; set; }        
        public string contentFormat { get; set; }
        public string contentName { get; set; }
        public string sessionName { get; set; }
        public string isBillable { get; set; }
        public double actualMins { get; set; }
        public double actualHours { get; set; }
        public double calculateDay { get; set; }
        public double workload { get; set; }
        public double billableDay { get; set; }
        public string projectID { get; set; }
        public string company { get; set; }
        public string bookBy { get; set; }
        public int year { get; set; }
        public string month { get; set; }
        public int contractTypeId { get; set; }
        public int contentBusinessTypeId { get; set; }
    }
}
