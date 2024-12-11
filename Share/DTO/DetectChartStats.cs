using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Share.DTO
{
    public class DetectChartStats
    {
        public Dictionary<int, int> MonthCounts { get; set; } // Key: Month, Value: Row Count
        public List<DetectChartRow> LatestRows { get; set; }  // List of the 5 latest rows
    }
}
