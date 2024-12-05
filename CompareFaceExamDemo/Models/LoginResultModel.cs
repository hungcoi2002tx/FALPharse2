using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthenExamCompareFace.Models
{
    public class LoginResultModel
    {
        public string token { get; set; }
        public string systemName { get; set; }
        public int? userRole { get; set; }
    }
}
