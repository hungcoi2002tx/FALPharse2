using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DemoSendWebhook
{
    public class WebhookClient
    {
        private static readonly HttpClient httpClient = new HttpClient();

        // Key bí mật giống với key mà webhook sử dụng
        private const string SecretKey = "your-secret-key";
        private const string WebhookUrl = "https://localhost:7217/api/webhookreceiver"; // URL của webhook

        public static async Task SendWebhookAsync()
        {
            // Tạo payload
            var payload = new WebhookPayload
            {
                ImageId = "IMG123456",
                SystemName = "ImageRecognitionSystem",
                Users = new[]
                {
                new UserInfo { UserId = "USER001", UserName = "Alice" },
                new UserInfo { UserId = "USER002", UserName = "Bob" }
            },
                Timestamp = DateTime.UtcNow
            };

            // Chuyển đổi payload sang chuỗi JSON
            string jsonPayload = JsonSerializer.Serialize(payload);

            // Tính chữ ký HMAC cho payload
            string signature = GenerateHMAC(jsonPayload, SecretKey);

            // Tạo HttpContent để gửi yêu cầu POST
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            // Thêm header "X-Signature" với chữ ký HMAC
            content.Headers.Add("X-Signature", signature);

            // Gửi yêu cầu POST đến webhook
            HttpResponseMessage response = await httpClient.PostAsync(WebhookUrl, content);

            // Kiểm tra phản hồi từ webhook
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Gửi dữ liệu thành công!");
            }
            else
            {
                Console.WriteLine($"Lỗi: {response.StatusCode}, Nội dung: {await response.Content.ReadAsStringAsync()}");
            }
        }

        // Hàm tạo chữ ký HMAC từ payload
        private static string GenerateHMAC(string payload, string secret)
        {
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret)))
            {
                var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
                return Convert.ToBase64String(hashBytes);
            }
        }

        // Payload gửi đến webhook
        public class WebhookPayload
        {
            public string ImageId { get; set; }      // ID của bức ảnh
            public string SystemName { get; set; }   // Tên hệ thống gửi
            public UserInfo[] Users { get; set; }    // Danh sách người dùng có trong ảnh
            public DateTime Timestamp { get; set; }  // Thời gian xảy ra sự kiện
        }

        // Thông tin người dùng
        public class UserInfo
        {
            public string UserId { get; set; }       // ID người dùng
            public string UserName { get; set; }     // Tên người dùng
        }
    }
}
