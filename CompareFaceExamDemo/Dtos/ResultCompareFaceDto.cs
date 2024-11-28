using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompareFaceExamDemo.Dtos
{
    public class ResultCompareFaceDto
    {
        public string? ExamCode { get; set; }
        public string? StudentCode { get; set; }
        public DateTime Time { get; set; } = DateTime.Now;
        public string? Status { get; set; }
        public string? Message { get; set; }
        public double Confidence { get; set; } = 0;
        public string? Note { get; set; }
    }
}
