using EOSServerDemo.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Text;

namespace EOSServerDemo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WebhookReceiverCompareFaceController : ControllerBase
    {
        private readonly string _secretKey;
        private readonly CompareFaceContext _compareFace;
        // Inject IConfiguration trực tiếp vào constructor
        public WebhookReceiverCompareFaceController(IConfiguration configuration, CompareFaceContext compareFace)
        {
            // Lấy giá trị SecretKey từ appsettings.json
            _secretKey = "your-secret-key";
            _compareFace = compareFace;

        }

        //[AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> ReceiveData([FromHeader(Name = "X-Signature")] string signature, [FromBody] ComparisonResult payload)
        {
            // Tạo chữ ký HMAC từ payload
            var payloadString = System.Text.Json.JsonSerializer.Serialize(payload);
            var computedSignature = GenerateHMAC(payloadString, _secretKey);

            // Kiểm tra chữ ký
            if (signature != computedSignature)
            {
                return Unauthorized("Chữ ký không hợp lệ.");
            }
            await UpdateResultAsync(payload);

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

        private async Task UpdateResultAsync(ComparisonResult payload)
        {
            // Tìm bản ghi Result theo ResultID
            var result = await _compareFace.Results.FindAsync(payload.ResultId);

            // Kiểm tra xem bản ghi có tồn tại không
            if (result != null)
            {
                // Cập nhật các trường
                // TODO: hỏi thắng về kiểu data
                if (payload.Similarity.HasValue)
                {
                    result.Confidence = payload.Similarity.Value;
                    if (payload.Similarity.Value > 60)
                    {
                        result.Status = "MATCH";
                    }
                    else
                    {
                        result.Status = "NOMATCH";
                    }
                }

                result.Message = "Đã hoàn thành";
                //result.Time = DateTime.Now;

                // Lưu thay đổi vào cơ sở dữ liệu
                await _compareFace.SaveChangesAsync();
            }
            else
            {
                // Xử lý trường hợp không tìm thấy bản ghi
                throw new KeyNotFoundException($"Result with ID {payload.ResultId} not found.");
            }
        }

    }
}
