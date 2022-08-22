using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nri_Webapplication_Backend.Models
{
    public class Type
    {
        public int Id { get; set; }
        public string Value { get; set; }
    }

    public class AvailableForContentFormat
    {
        public int Id { get; set; }
        public string Value { get; set; }
    }
    public class RoomModel : BaseModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Type type { get; set; }
        public string InstancyLocationID { get; set; }
        public string InstancyDisplayName { get; set; }
        public AvailableForContentFormat AvailableForContentFormat { get; set; }
        public byte IsActive { get; set; }

    }
}
