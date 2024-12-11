using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Share.DTO
{
    public class TrainChartStats
    {
        public Dictionary<int, int> MonthCounts { get; set; } // Key: Month, Value: Row Count
        public List<TrainChartRow> LatestRows { get; set; } 
    }
}
