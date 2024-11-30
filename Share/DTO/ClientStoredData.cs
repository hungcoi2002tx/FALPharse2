using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Share.DTO
{
    public class ClientStoredData
    {
        public string? FileName { get; set; } = null;
        public FaceDetectionResult? Data { get; set; } = null;
        public string CreateDate { get; set; } = null!;
    }
}
