using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Share.SystemModel
{
    public class CompareResponseResult
    {
        public float? Percentage { get; set; }
        /// <summary>
        /// API data 
        /// </summary>
        public string? Message { get; set; }
    }
}
