using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nri_Webapplication_Backend.Models
{
    public class CourseSummaryModel
    {
        public string contentID { get; set; }
        public string course { get; set; }    
        public string monthNo { get; set; }
        public string monthName { get; set; }
        public string year { get; set; }
        public int numberOfCourse { get; set; }
    }
}
