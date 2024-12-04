using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AuthenExamTakePicture.Models.DTOs
{
    public class SaveImageDTO
    {
        public string ImageBase { get; set; }
        public string ExamCode { get; set; }
        public string StudentCode { get; set; }
    }
}
