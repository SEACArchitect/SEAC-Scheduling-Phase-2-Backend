using System;
using System.Collections.Generic;

namespace Nri_Webapplication_Backend.DTO
{
    public partial class TrainerMaster
    {
        public TrainerMaster()
        {
            EventScheduleModerator = new HashSet<EventScheduleModerator>();
            EventScheduleTrainer = new HashSet<EventScheduleTrainer>();
            ModeratorSkill = new HashSet<ModeratorSkill>();
            ModeratorSupportBusinessType = new HashSet<ModeratorSupportBusinessType>();
            ModeratorSupportRoomType = new HashSet<ModeratorSupportRoomType>();
        }

        public int TrainerId { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public string NickNameEn { get; set; }
        public string NickNameTh { get; set; }
        public string Telephone { get; set; }
        public string Email { get; set; }
        public string ICalendarLink { get; set; }
        public int TrainerTypeId { get; set; }
        public int TraninerContractTypeId { get; set; }
        public byte IsActive { get; set; }
        public int MaximumEventPerMonth { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public int? UpdatedBy { get; set; }
        public byte IsTrainer { get; set; }

        public virtual TrainerType TrainerType { get; set; }
        public virtual TrainerContractType TraninerContractType { get; set; }
        public virtual ICollection<EventScheduleModerator> EventScheduleModerator { get; set; }
        public virtual ICollection<EventScheduleTrainer> EventScheduleTrainer { get; set; }
        public virtual ICollection<ModeratorSkill> ModeratorSkill { get; set; }
        public virtual ICollection<ModeratorSupportBusinessType> ModeratorSupportBusinessType { get; set; }
        public virtual ICollection<ModeratorSupportRoomType> ModeratorSupportRoomType { get; set; }
    }
}
