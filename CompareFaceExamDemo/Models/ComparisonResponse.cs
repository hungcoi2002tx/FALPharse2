using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Share.SystemModel;

namespace CompareFaceExamDemo.Models
{
    public class ComparisonResponse
    {
        public int Status { get; set; }
        public string? Message { get; set; }
        public CompareResponseResult? Data { get; set; }
    }
}
