using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nri_Webapplication_Backend.Models
{
    public class ContentMasterTemplate
    {
        public string ContentCode { get; set; }
        public string ContentName { get; set; }
        public string DisplayName { get; set; }
        public string OriginalContentName { get; set; }
        public string OutLineID { get; set; }
        public string CourseID { get; set; }
        public string CourseTitle { get; set; }
        public string Duration { get; set; }
        public string Paxmax { get; set; }
        public string IsInternal { get; set; }
        public string BusinessType { get; set; }
        public string LearningType { get; set; }
        public string ContentFormat { get; set; }
        public string Active { get; set; }
    }
    public class TrainerAndModerator
    {
        public string Name { get; set; }
        public string LName { get; set; }
        public string NName { get; set; }
        public string NNameTh { get; set; }
        public string Telno { get; set; }
        public string Email { get; set; }
        public string Contract { get; set; }
        public string Type { get; set; }
        public string Trainer { get; set; }
        public string Active { get; set; }
    }
    public class RCertifiedTrainer
    {
        public string ContentName { get; set; }
        public string LearningTypeName { get; set; }
        public string ContentBusinessTypeName { get; set; }
        public string TrainerName { get; set; }
        public string TraninerContractTypeName { get; set; }
        public string Active { get; set; }
    }
    public class CourseSession
    {
        public int? EventScheduleID { get; set; }
        public int? ContentID { get; set; }
        public string CourseOutlineId { get; set; }
        public string Seats { get; set; }
        public string Language { get; set; }
        public string WebinarTool { get; set; }
        public string Location { get; set; }
        public string ParticipantURL { get; set; }
        public string StartDate { get; set; }
        public string StartMonth { get; set; }
        public string StartYear { get; set; }
        public string StartTime { get; set; }
        public string EndDate { get; set; }
        public string EndMonth { get; set; }
        public string EndYear { get; set; }
        public string EndTime { get; set; }
        public string TimeZoneUTC { get; set; }
        public string InstructorEmails { get; set; }
        public string PrivateClass { get; set; }
        public string ModeratorEmails { get; set; }
        //public int ContentBusinessType { get; set; }
    }
    public class CourseSessionFile
    {
        public string CourseOutlineId { get; set; }
        public string Seats { get; set; }
        public string Language { get; set; }
        public string WebinarTool { get; set; }
        public string Location { get; set; }
        public string ParticipantURL { get; set; }
        public string StartDate { get; set; }
        public string StartMonth { get; set; }
        public string StartYear { get; set; }
        public string StartTime { get; set; }
        public string EndDate { get; set; }
        public string EndMonth { get; set; }
        public string EndYear { get; set; }
        public string EndTime { get; set; }
        public string TimeZoneUTC { get; set; }
        public string InstructorEmails { get; set; }
        public string PrivateClass { get; set; }
        public string ModeratorEmails { get; set; }
        //public int ContentBusinessType { get; set; }
    }
    public class CourseOutline
    {
        public string CreatedAt { get; set; }
        public string CourseOutlineId { get; set; }
        public string Title { get; set; }
        public string Category { get; set; }
        public string CourseTitle { get; set; }
        public int? ContentId { get; set; }
    }
    public class CourseOutlineFile
    {
        public string CreatedAt { get; set; }
        public string CourseOutlineId { get; set; }
        public string Title { get; set; }
        public string Category { get; set; }
        public string CourseTitle { get; set; }
    }
    public class Instructors
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

}
