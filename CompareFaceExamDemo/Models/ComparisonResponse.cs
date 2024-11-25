using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompareFaceExamDemo.Models
{
    public class ComparisonResponse
    {
        public int Status { get; set; }
        public string? Message { get; set; }
        public ComparisonResult? Data { get; set; }
    }
}
