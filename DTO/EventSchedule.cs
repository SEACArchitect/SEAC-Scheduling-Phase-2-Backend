using System;
using System.Collections.Generic;

namespace Nri_Webapplication_Backend.DTO
{
    public partial class EventSchedule
    {
        public EventSchedule()
        {
            EventScheduleModerator = new HashSet<EventScheduleModerator>();
            EventScheduleTrainer = new HashSet<EventScheduleTrainer>();
        }

        public int EventScheduleId { get; set; }
        public int ContentId { get; set; }
        public int EventRoomId { get; set; }
        public int? EventCategoryId { get; set; }
        public int LearningTypeId { get; set; }
        public int ContentFormatId { get; set; }
        public DateTime StartTime { get; set; }
        public int? Duration { get; set; }
        public string Link { get; set; }
        public string DisplayName { get; set; }
        public string ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string Company { get; set; }
        public int? NoOfParticipant { get; set; }
        public string Session { get; set; }
        public byte? IsBillable { get; set; }
        public string Remark { get; set; }
        public byte? IsEmailSent { get; set; }
        public int LanguageId { get; set; }
        public DateTime UploadToInstancyDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime? EndTime { get; set; }
        public byte IsActive { get; set; }
        public int ContentBusinessTypeId { get; set; }
        public string Location { get; set; }
        public string BookBy { get; set; }
        public int? StatusId { get; set; }
        public DateTime? CancelledDate { get; set; }
        public int? CancelledBy { get; set; }
        public int IsModerator { get; set; }

        public virtual ContentMaster Content { get; set; }
        public virtual ContentBusinessType ContentBusinessType { get; set; }
        public virtual ContentFormatMaster ContentFormat { get; set; }
        public virtual EventCategory EventCategory { get; set; }
        public virtual EventRoomMaster EventRoom { get; set; }
        public virtual Language Language { get; set; }
        public virtual LearningTypeMaster LearningType { get; set; }
        public virtual Status Status { get; set; }
        public virtual ICollection<EventScheduleModerator> EventScheduleModerator { get; set; }
        public virtual ICollection<EventScheduleTrainer> EventScheduleTrainer { get; set; }
    }
}
