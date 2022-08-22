using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nri_Webapplication_Backend.Models
{
    public class MailModel
    {
        public MailModel()
        {
            Moderator = new List<string>();
        }
        public int ScheduleId { get; set; }
        public string Schedule { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string Trainer { get; set; }
        public List<string> Moderator { get; set; }
        public string Link { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
    public class RequesModerator
    {
        public string ContentName { get; set; }
        public string BusinessType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime OldStartDate { get; set; }
        public DateTime OldEndDate { get; set; }
        public string RoomName { get; set; }
        public string OldRoomName { get; set; }
        public string Location { get; set; }
        public string Link { get; set; }
        public string Session { get; set; }
        public string Status { get; set; }
        public string Company { get; set; }
        //public string ProjectIDoCost { get; set; }
        public string ProjectName { get; set; }
        public string Participant { get; set; }
        public string Remark { get; set; }
        public string GotoLink { get; set; }
        public int DateChange { get; set; }
    }
}
