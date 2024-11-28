using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Share.DTO
{
    public class TrainStatsResponse
    {
        public int TotalTrainedUserId { get; set; }
        public int TotalTrainedFaceId { get; set; }
        public List<TrainStatsOfUser> UserStats { get; set; }
    }
}
