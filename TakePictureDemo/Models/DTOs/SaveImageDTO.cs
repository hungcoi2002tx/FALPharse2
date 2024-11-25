using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TakePictureDemo.Models.DTOs
{
    public class SaveImageDTO
    {
        public string ImageBase { get; set; }
        public string ExamCode { get; set; }
        public string StudentCode { get; set; }
    }
}
