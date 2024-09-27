using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Share.SystemModel
{
    public static class GlobalVarians
    {
        public static readonly string SystermId = "FUALUMNI";
        public static readonly long MAXFILESIZE = 15L * 1024 * 1024 * 1024;
        public static readonly long PARTSIZE = 500 * 1024 * 1024;
        public static readonly long DIVIDESIZE = 500 * 1024 * 1024;
    }
}
