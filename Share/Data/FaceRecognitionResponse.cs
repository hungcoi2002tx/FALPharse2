using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Share.Data
{
    public class FaceRecognitionResponse
    {
        public string? TimeAppearances { get; set; } = null;
        public BoundingBox? BoundingBox { get; set; } = null;
        public string FaceId { get; set; } = null!;
        public string? UserId { get; set; } = null;
    }
}
