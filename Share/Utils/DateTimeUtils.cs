using Share.Constant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Share.Utils
{
    public static class DateTimeUtils
    {
        public static string GetDateTimeVietNamNow()
        {
            TimeZoneInfo vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById(SystemConstants.TIME_ZONE_VIET_NAM);
            DateTime vietnamTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone);
            var result = vietnamTime.ToString(SystemConstants.DATE_FORMAT);
            return result;
        }
    }
}
