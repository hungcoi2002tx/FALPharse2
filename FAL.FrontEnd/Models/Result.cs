namespace FAL.FrontEnd.Models
{
    public class Result
    {
        public string FileName { get; set; }
        public string SystemName { get; set; }
        public DateTime CreateDate { get; set; }
        public FileData Data { get; set; }
    }
}
