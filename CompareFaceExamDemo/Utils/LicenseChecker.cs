using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthenExamCompareFaceExam.Utils
{
    public class LicenseChecker
    {
        private static readonly HttpClient client = new HttpClient();

        public static async Task<bool> CheckLicenseKeyAsync(string licenseKey)
        {
            // Địa chỉ endpoint Google Apps Script
            string url = string.Format("https://script.google.com/macros/s/AKfycbwxwkyig3EnEovXV0eWy2e6v9VW_RKee0L8moWaGw2xhQrJo4mknXGEXSaOMIKWNhUY/exec?licenseKey={0}", licenseKey);

            try
            {
                // Gửi request đến Google Apps Script
                var response = await client.GetStringAsync(url);

                // Kiểm tra kết quả trả về
                return response.Trim().Equals("true", StringComparison.CurrentCultureIgnoreCase);
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
