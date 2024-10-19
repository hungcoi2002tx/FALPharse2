using FALWebhook.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;

namespace FALWebhook.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WebhookReceiverController : ControllerBase
    {
        private readonly string _secretKey;

        // Inject IConfiguration trực tiếp vào constructor
        public WebhookReceiverController(IConfiguration configuration)
        {
            // Lấy giá trị SecretKey từ appsettings.json
            _secretKey = configuration["WebhookSettings:SecretKey"];
        }
        [HttpPost]
        public IActionResult ReceiveData([FromHeader(Name = "X-Signature")] string signature, [FromBody] WebhookPayload payload)
        {
            // Tạo chữ ký HMAC từ payload
            var payloadString = System.Text.Json.JsonSerializer.Serialize(payload);
            var computedSignature = GenerateHMAC(payloadString, _secretKey);

            // Kiểm tra chữ ký
            if (signature != computedSignature)
            {
                return Unauthorized("Chữ ký không hợp lệ.");
            }

            // Xử lý dữ liệu từ payload
            Console.WriteLine($"Nhận được bức ảnh ID: {payload.ImageId}");
            Console.WriteLine($"Hệ thống gửi: {payload.SystemName}");
            Console.WriteLine($"Thời gian: {payload.Timestamp}");

            if (payload.Users != null && payload.Users.Count > 0)
            {
                Console.WriteLine("Danh sách người dùng có trong ảnh:");
                foreach (var user in payload.Users)
                {
                    Console.WriteLine($"- ID: {user.UserId}, Tên: {user.UserName}");
                }
            }
            else
            {
                Console.WriteLine("Không có người dùng nào trong ảnh.");
            }

            // Trả về phản hồi cho client
            return Ok(new { Message = "Webhook đã nhận dữ liệu thành công." });
        }

        private static string GenerateHMAC(string payload, string secret)
        {
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret)))
            {
                var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
                return Convert.ToBase64String(hashBytes);
            }
        }
    }
   

}
