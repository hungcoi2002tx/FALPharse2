using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Share.DTO
{
    public class RequestStatsResponse
    {
        public int TotalSuccess { get; set; }
        public int TotalFailed { get; set; }
        public List<GroupedRequestData> RequestData { get; set; } = new List<GroupedRequestData>();
    }
}
