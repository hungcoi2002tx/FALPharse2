using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthenExamCompareFaceExam.Dtos
{
    public class UpdateFileResult
    {
        public string? FilePath { get; set; }
        public string? Message { get; set; }
        public string? Status { get; set; } = null;
    }
}
