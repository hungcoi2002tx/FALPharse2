namespace FAL.FrontEnd.Models
{
    public class UserTrain
    {
        public Guid UserId { get; set; }
        public Guid FaceId { get; set; }
        public string SystemName { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
