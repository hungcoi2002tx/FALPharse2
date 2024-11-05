using EOSServerDemo.Models;

namespace EOSServerDemo.Dtos
{
    public class ResultCompareFaceDto
    {
        public string? StudentCode { get; set; }
        public string? Status { get; set; }
        public double Confidence { get; set; }
        public string? ExamCode { get; set; }
        public DateTime Time { get; set; }
        public string? Note { get; set; }
        public string? Message { get; set; }
    }
}
