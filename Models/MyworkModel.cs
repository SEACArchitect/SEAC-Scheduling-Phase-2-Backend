using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nri_Webapplication_Backend.Models
{
    public class MyworkModel
    {
        public MyworkModel()
        {
            trainer = new List<TrainerWork>();
        }
        public string NickNameTH { get; set; }
        public string NickNameEN { get; set; }
        public string FullName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string RoomName { get; set; }
        public int ContentID { get; set; }
        public string ContentName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string Language { get; set; }
        public byte isActiveSchedule { get; set; }
        public byte isTrainer { get; set; }
        public string Link { get; set; }
        public int ContentBusinessTypeID { get; set; }
        public string ContentBusinessTypeName { get; set; }
        public string ContentFormantName { get; set; }
        public string Location { get; set; }
        public string Session { get; set; }
        public string Company { get; set; }
        public string ProjectName { get; set; }
        public int CreatedBy { get; set; }
        public string Remark { get; set; }
        public List<TrainerWork> trainer { get; set; }
        public int EventScheduleID { get; set; }

        public string BookBy { get; set; }

        public byte isActiveEventScheduleTrainer { get; set; }
        public byte isActiveScheduleModerator { get; set; }

        public int? statusID { get; set; }
        public string status { get; set; }
    }

    public class TrainerWork
    {
        public int ID { get; set; }
        public string name { get; set; }
        public byte isTrainer { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Telephone { get; set; }
    }
}
