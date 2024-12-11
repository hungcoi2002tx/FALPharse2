namespace FAL.FrontEnd.Models
{
    public class Face
    {
        public Guid? FaceId { get; set; }
        public Guid? UserId { get; set; }
        public BoundingBox BoundingBox { get; set; }
        public string TimeAppearances { get; set; }
    }
}
