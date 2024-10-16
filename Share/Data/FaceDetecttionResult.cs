using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Share.Data
{
    public class FaceDetectionResult
    {
        public string FileName { get; set; } = null!;
        public List<FaceRecognitionResponse> RegisteredFaces { get; set; } = null!;
        public List<FaceRecognitionResponse>? UnregisteredFaces { get; set; } = null;
        public int Width { get; set; }
        public int Height { get; set; }
        public string Key { get; set; }
    }
}
