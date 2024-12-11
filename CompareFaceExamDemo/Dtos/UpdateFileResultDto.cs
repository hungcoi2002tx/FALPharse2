using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthenExamCompareFace.Dtos
{
    public class UpdateFileResultDto
    {
        public string? FilePath { get; set; }
        public string? Message { get; set; }
        public string? Status { get; set; } = null;
        public string? StudentNumber { get; set; } = null;
    }
}
