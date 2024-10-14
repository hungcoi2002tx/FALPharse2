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
            TimeZoneInfo vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById(Constants.TIME_ZONE_VIET_NAM);
            DateTime vietnamTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone);
            var result = vietnamTime.ToString(Constants.DATE_FORMAT);
            return result;
        }
    }
}
