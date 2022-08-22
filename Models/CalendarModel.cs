using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
//using System.Data;

namespace Nri_Webapplication_Backend.Models
{
    public class DwnLoadIcs : BaseModel
    {
        public string IcsPath { get; set; }
        //public DataTable eventData { get; set; }
        //public DateTime SyncDate { get; set; }
    }

    public class Calendar
    {
        public string Summary { get; set; }
        public DateTime DtStart { get; set; }
        public DateTime DtEnd { get; set; }
        public string Location { get; set; }
    }

    public class RoomParing : BaseModel
    {
        public string Room { get; set; }
        public string RoomName { get; set; }
        public string Link { get; set; }
        public DateTime ClassStart { get; set; }
        public DateTime ClassEnd { get; set; }
        public string Topic { get; set; }
    }

    public class EditLink : Link
    {
        public DateTime EditDate { get; set; }
        public int EditBy { get; set; }
    }

    public class Link : BaseModel
    {
        public int IdLink { get; set; }
        public string IcsLink { get; set; }
        public int Status { get; set; }
    }

    public class Classroom
    {
        public int? PairingId { get; set; }
        public string RoomCode { get; set; }
        public int? BizTypeId { get; set; }
        public int? EventRoomId { get; set; }
        public string RoomName { get; set; }
        public string LinkZoomClassRoom { get; set; }
        public string Topic { get; set; }
        public DateTime? ClassStart { get; set; }
        public DateTime? ClassEnd { get; set; }
        public int? CalendarIdLink { get; set; }
        public int? ApproveStatus { get; set; }
        public int? EventScheduleId { get; set; }
        public DateTime? ScheduleDateStart { get; set; }
        public DateTime? ScheduleDateEnd { get; set; }
    }
    public class GetClassRoom
    {
        //public DateTime StartSearchDate { get; set; }
        //public DateTime EndSearchDate { get; set; }
        public int BusinessTypeId { get; set; } 
        public string Search { get; set; }
    }

    public class ApprParams
    {
        public int? PairingId { get; set; }
        public int ApprStat { get; set; }
        public string ZoomLink { get; set; }
        public string Topic { get; set; }
        public int RoomId { get; set; }
        public string RoomName { get; set; }
        public int EventScheduleId { get; set; }
        //public int IsPairing { get; set; }
    }

    public class ListApprParams
    {
        public ListApprParams()
        {
            ParamsList = new List<ApprParams>();
        } 
        
        public List<ApprParams> ParamsList { get; set; }

    }

    public class SearchClassroomOverlaping
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

}
