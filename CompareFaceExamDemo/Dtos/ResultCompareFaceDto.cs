using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthenExamCompareFace.Dtos
{
    public class ResultCompareFaceDto
    {
        public int Id { get; set; }
        public string? ExamCode { get; set; }
        public string? StudentCode { get; set; }
        public DateTime Time { get; set; } = DateTime.Now;
        public ResultStatus Status { get; set; }
        public string? Message { get; set; }
        public double Confidence { get; set; } = 0;
        public string? Note { get; set; }
        public string? ImageTagetPath { get; set; } = null;
        public string? ImageSourcePath { get; set; } = null;
    }

    public class ResultCompareFaceTxtDto
    {
        public int Id { get; set; }
        public string? ExamCode { get; set; }
        public string? StudentCode { get; set; }
        public DateTime Time { get; set; } = DateTime.Now;
        public int Status { get; set; }
        public string? Message { get; set; }
        public double Confidence { get; set; } = 0;
        public string? Note { get; set; }
        public string? ImageTagetPath { get; set; } = null;
        public string? ImageSourcePath { get; set; } = null;
    }

    public enum ResultStatus
    {
        PROCESSING,  // Đang xử lý
        MATCHED,     // Khớp
        NOTMATCHED   // Không khớp
    }
}
