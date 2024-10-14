using FALWebhook.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

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
            _secretKey = "your-secret-key";
        }
        [HttpPost]
        public async Task<IActionResult> ReceiveData([FromHeader(Name = "X-Signature")] string signature, [FromBody] FaceDetectionResult payload)
        {
            // Tạo chữ ký HMAC từ payload
            var payloadString = System.Text.Json.JsonSerializer.Serialize(payload);
            var computedSignature = GenerateHMAC(payloadString, _secretKey);

            // Kiểm tra chữ ký
            if (signature != computedSignature)
            {
                return Unauthorized("Chữ ký không hợp lệ.");
            }
            string botToken = "7866949652:AAErC16gTxmjl6pVMCf0TeTOaiDjG55TRME";
            string chatId = "-1002303179397";
            await SendMessageTele.TelegramBot.SendMessageAsync(botToken, chatId, payloadString);

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
