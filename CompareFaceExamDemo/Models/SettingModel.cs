using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthenExamCompareFace.Models
{
    public class SettingModel
    {
        public int Confident { get; set; }
        public int NumberOfThread { get; set; }
        public int NumberOfRetry { get; set; }
        public string DirectoryImageSource { get; set; }
    }
}
