using Microsoft.OpenApi.Writers;
using Nri_Webapplication_Backend.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Nri_Webapplication_Backend.Models
{
    public class TypeTrainer
    {
        public int TrainerID { get; set; }
        public int moderatorID
        {
            get;
            set;
        }
        public class ContentName
        {
            public int Id { get; set; }
            public string Value { get; set; }
            public decimal Duration { get; set; }
        }
        public class Trainer
        {
            public int? Id { get; set; }
            public string Name { get; set; }
            public string lastname { get; set; }
            public int MaxEventPerMonth { get; set; }

            public byte IsTrainer { get; set; }
            public byte IsActive { get; set; }
        }
        public class Moderator
        {
            public int? Id { get; set; }
            public string Name { get; set; }
            public string Nickname { get; set; }
            public string lastname { get; set; }
            public int EventCount { get; set; }
            public int MaxEventPerMonth { get; set; }
            public byte isTrainer { get; set; }
            public byte IsActive { get; set; }
        }
        public class CurMod
        {
            public int? ModeratorId { get; set; }
            public string Name { get; set; }
            public string Nickname { get; set; }
            public string lastname { get; set; }
            //public int MaxEventPerMonth { get; set; }
            //public byte isTrainer { get; set; }
            //public byte IsActive { get; set; }
        }
        public class Room
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public byte IsActive { get; set; }
        }
        public class Category
        {
            public int? Id { get; set; }
            public string Value { get; set; }
        }

        public class LanguagePlanner
        {
            public int Id { get; set; }
            public string Value { get; set; }
        }
        public class PlannerModel
        {
            public PlannerModel()
            {
                //Moderator = new List<moderator>();
            }
            public int Id { get; set; }
            public BusinessType BusinessType { get; set; }
            public LearningType LearningType { get; set; }
            public ContentFormat ContentFormat { get; set; }
            public string Lang { get; set; }

            public LanguagePlanner Language { get; set; }
            public Category Category { get; set; }
            public ContentName ContentName { get; set; }
            public DateTime StartTime { get; set; }
            public DateTime? EndTime { get; set; }
            public Trainer? Trainer { get; set; }


            public List<Moderator> Moderator { get; set; }
            public Room Room { get; set; }
            public byte IsActive { get; set; }
            public byte? IsTrainerAccept { get; set; }
            public byte? IsTrainerReply { get; set; }
            public byte? IsSendEmail { get; set; }

            // Get By scheduleID
            public byte? IsBillable { get; set; }
            public string ProjectName { get; set; }
            public string ProjectId { get; set; }
            public string Remark { get; set; }
            public string Link { get; set; }
            public string Company { get; set; }
            public string BookBy { get; set; }
            public string Location { get; set; }
            public string Session { get; set; }
            public int? NoOfParticipant { get; set; }
            public StatusPlanner status { get; set; }
            public DateTime uploadDate { get; set; }
            public ContactTypePlanner contractType { get; set; }
            public int EventCount { get; set; }
            public int IsModerator { get; set; }

            // For condition 

            [JsonIgnore]
            public byte IsSchedule { get; set; }

            [JsonIgnore]
            public byte IsActiveTrainer { get; set; }

            [JsonIgnore]
            public byte? IsActiveModerator { get; set; }
        }
    }
}
