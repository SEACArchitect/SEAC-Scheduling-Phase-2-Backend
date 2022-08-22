using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nri_Webapplication_Backend.Models
{
    public class RoomTypePlanner
    {
        public int? Id { get; set; }
        public string Value { get; set; }
    }

    public class ContentFormatPlanner
    {
        public int Id { get; set; }
        public string Value { get; set; }
    }
    public class RoomPlanner
    {
        public int? Id { get; set; }
        public string Value { get; set; }

        public RoomTypePlanner Type { get; set; }
        public ContentFormatPlanner ContentFormat { get; set; }
    }
    public class StatusPlanner
    {
        public int? Id { get; set; }
        public string Value { get; set; }
    }
    public class TrainerContacTypeCombo
    {
        public int Id { get; set; }
        public string name { get; set; }
        public string lastname { get; set; }
        public string nickname { get; set; }
        public int eventCount { get; set; }
        public int maxEventPerMonth { get; set; }
        public ContactTypePlanner contractType { get; set; }
    }

    public class TrainerPlanner
    {
        public int Id { get; set; }
        public string Value { get; set; }
    }

    public class ContactTypePlanner
    {
        public int Id { get; set; }
        public string Value { get; set; }
    }

    public class ContentPlanner
    {
        public int Id { get; set; }
        public string Value { get; set; }
        public Decimal Duration { get; set; }
    }

    public class PlannerModel_Request
    {
        public int? EventScheduleId { get; set; }
        public int BusinessTypeId { get; set; } //
        public int LearningTypeId { get; set; }//
        public int ContentFormatId { get; set; }//
        public int LangId { get; set; }//
        public int ContentId { get; set; }//
        public int? CategoryId { get; set; }//
        public DateTime UploadDate { get; set; }//
        public string ProjectName { get; set; }
        public string ProjectId { get; set; }
        public string Company { get; set; }
        public string BookBy { get; set; }
        public byte? IsBillable { get; set; }
        public DateTime StartDate { get; set; }//
        public DateTime EndDate { get; set; }//
        public int TrainerId { get; set; }//
        public string TrainerName { get; set; }
        public List<int> Moderator { get; set; }//
        public int RoomId { get; set; }//
        public string RoomName { get; set; }
        public string Location { get; set; }
        public string Session { get; set; }
        public int? NoOfParticipant { get; set; }
        public int? StatusId { get; set; }
        public int CreatedBy { get; set; }
        public int UpdatedBy { get; set; }
        public string Link { get; set; }//
        public string Remarks { get; set; }//
        public int IsModerator { get; set; }


    }
}
