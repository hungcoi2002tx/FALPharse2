using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthenExamCompareFace.Utils
{
    public static class StringHelper
    {
        public static bool IsNotNullOrEmpty(this object input)
        {
            if (input == null || input.ToString().Length == 0 || input.ToString().Trim().Length == 0)
            {
                return false;
            }
            return true;
        }

        public static bool IsNullOrEmpty(this object input)
        {
            if (input == null || input.ToString().Length == 0 || input.ToString().Trim().Length == 0)
            {
                return true;
            }
            return false;
        }
    }
}
