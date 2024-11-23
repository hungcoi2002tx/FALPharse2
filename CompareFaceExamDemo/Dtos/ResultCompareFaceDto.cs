using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompareFaceExamDemo.Dtos
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
        public string? TargetImagePath { get; set; }
        public string? SourceImagePath { get; set; }
    }
}
