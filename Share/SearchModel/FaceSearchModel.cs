using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Share.SearchModel
{
    public class FaceSearchModel
    {
        public string? FaceId { get; set; }
        public string? UserId { get; set; }
        public string SystemId { get; set; }    
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
