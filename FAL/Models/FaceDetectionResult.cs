﻿namespace FALWebhook.Models
{


    public class FaceRecognitionResponse
    {
        public string? TimeAppearances { get; set; } = null;
        public BoundingBox? BoundingBox { get; set; } = null;
        public string? FaceId { get; set; } = null!;
        public string? UserId { get; set; } = null;
    }

public class BoundingBox
    {
        public float? Left { get; set; }
        public float? Top { get; set; }
        public float? Width { get; set; }
        public float? Height { get; set; }
    }
 
public class FaceDetectionResult
    {
        public string? FileName { get; set; } = null!;
        public List<FaceRecognitionResponse>? RegisteredFaces { get; set; } = null!;
        public List<FaceRecognitionResponse>? UnregisteredFaces { get; set; } = null;
    }
}
