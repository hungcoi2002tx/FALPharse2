using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Share.DTO
{
    public class CollectionChartStats
    {
        public int UserCount { get; set; } // Number of unique users
        public int FaceCount { get; set; } // Number of unique face IDs
        public int MediaCount { get; set; } // Total media detected
    }
}
