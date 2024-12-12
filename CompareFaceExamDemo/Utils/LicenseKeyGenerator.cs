using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;
using System.Security.Cryptography;

namespace AuthenExamCompareFace.Utils
{
    public class LicenseKeyGenerator
    {
        public static string GetLicenseKey()
        {
            string? hwid = null;
            string bios = GetBios() ?? throw new Exception("Không thể lấy BIOS");
            string processorId = GetProcessorId() ?? throw new Exception("Không thể lấy BIOS");
            hwid = bios + "_-" + processorId;
            string licenseKey = GenerateKey(hwid) ?? throw new Exception("Không thể lấy license key");
            return licenseKey;
        }

        private static string? GetProcessorId()
        {
            string? processorId = null;
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT ProcessorId FROM Win32_Processor");
            ManagementObjectCollection info = searcher.Get();
            foreach (ManagementObject obj in info)
            {
                processorId = obj["ProcessorId"].ToString();
                break;
            }
            return processorId;
        }

        private static string? GetBios()
        {
            string? bios = null;
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_BIOS");
            ManagementObjectCollection info = searcher.Get();
            foreach (ManagementObject obj in info)
            {
                bios = obj["SerialNumber"].ToString();
                break;
            }
            return bios;
        }

        private static string GenerateKey(string hwid)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] sourceBytes = Encoding.UTF8.GetBytes(hwid);
                byte[] hashBytes = sha256Hash.ComputeHash(sourceBytes);
                StringBuilder builder = new StringBuilder();
                foreach (byte b in hashBytes)
                {
                    builder.Append(b.ToString("x2")); // Chuyển byte sang hex
                }
                return builder.ToString().Substring(0, 25);
            }
        }
    }
}
