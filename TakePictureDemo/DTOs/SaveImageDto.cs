using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AuthenExamTakePicture.DTOs
{
    public class SaveImageDto
    {
        public string ImageBase { get; set; }
        public string ExamCode { get; set; }
        public string StudentCode { get; set; }
    }
}
