using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nri_Webapplication_Backend.Models
{
    public class BusinessType
    {
        public int Id { get; set; }
        public string Value { get; set; }
        public string Name { get;set; }
    }
    public class LearningType
    {
        public int Id { get; set; }
        public string Value { get; set; }
    }
    public class TrainerList
    {
        public int Id { get; set; }
        public string Value { get; set; }
    }
    public class ContentFormat
    {
        public int Id { get; set; }
        public string Value { get; set; }

    }
    public class ContentMasterModel : BaseModel
    {

        public ContentMasterModel()
        {
            TrainerList = new List<TrainerList>();
            ContentFormat = new List<ContentFormat>();
        }
        public int? Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string OriginalName { get; set; }
        public Decimal Duration { get; set; }
        public byte IsActive { get; set; }
        public int PaxMax { get; set; }
        public byte? IsPrivillege { get; set; }
        public byte? IsInternal { get; set; }
        public string OutLineId { get; set; }
        public string CourseId { get; set; }
        public string CourseTitle { get; set; }
        public BusinessType BusinessType
        { get; set; }
        public LearningType LearningType { get; set; }

        public List<TrainerList> TrainerList { get; set; }

        public List<ContentFormat> ContentFormat { get; set; }

    }
}
