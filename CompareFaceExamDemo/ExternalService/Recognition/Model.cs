using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthenExamCompareFaceExam.ExternalService.Recognition
{
    public class LoginRequestModel
    {
        public string username { get; set; }
        public string password { get; set; }
    }

    public class LoginResultModel
    {
        public string token { get; set; }
        public string systemName { get; set; }
        public int? userRole { get; set; }
    }
}
