﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alumniphase2.Lambda.Models
{
    public class FaceDetectionResult
    {
        public string FileName { get; set; } = null!;
        public List<FaceRecognitionResponse> RegisteredFaces { get; set; } = null!;
        public List<FaceRecognitionResponse>? UnregisteredFaces { get; set; } = null;
        public int? ImageWidth { get; set; } = null;
        public int? ImageHeight { get; set; } = null;
        public string Key { get; set; } = null!;
    }
}
