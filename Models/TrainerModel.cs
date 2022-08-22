using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nri_Webapplication_Backend.Models
{
    public class TrainerType
    {
        public int id { get; set; }
        public string value { get; set; }
    }
    public class ContractType
    {
        public int id { get; set; }
        public string value { get; set; }
    }
    public class Skill
    {
        public int id { get; set; }
        public string value { get; set; }
    }

    public class RoomType
    {
        public int id { get; set; }
        public string value { get; set; }
    }
    public class TrainerModel : BaseModel
    { 
        public TrainerModel()
        {
            businessType = new List<BusinessType>();
            skill = new List<Skill>();
            roomType = new List<RoomType>();

        }
        public int id { get; set; }
        public string name { get; set; }
        public string lastname { get; set; }
        public string nicknameTH { get; set; }
        public string nicknameEN { get; set; }
        public string tel { get; set; }
        public string email { get; set; }
        public TrainerType type  { get; set; }
        public ContractType contractType { get; set; }
        public int maxEventPerMonth { get; set; }
        public byte isActive { get; set; }
        public byte isTrainer { get; set; }
        public string iCalendarLink { get; set; }
        public List<BusinessType> businessType { get; set; }
        public List<Skill> skill { get; set; }
        public List<RoomType> roomType { get; set; }
        
    }
}
