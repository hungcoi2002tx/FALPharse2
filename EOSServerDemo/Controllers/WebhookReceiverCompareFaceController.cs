using EOSServerDemo.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using AuthenExamCompareFaceExam.DAO;
using AuthenExamCompareFaceExam.Entities;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace EOSServerDemo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WebhookReceiverCompareFaceController : ControllerBase
    {
        private readonly string _secretKey;
        private readonly ExamDAO<EOSComparisonResult> _examDAO;

        // Inject IConfiguration và ExamDAO vào constructor
        public WebhookReceiverCompareFaceController(IConfiguration configuration, ExamDAO<EOSComparisonResult> examDAO)
        {
            _secretKey = "your-secret-key";
            _examDAO = examDAO;
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
            // Xử lý payload và cập nhật thông tin vào file sử dụng ExamDAO
            string examDate = "2024-11-10"; // Cần lấy thông tin đợt thi từ payload hoặc qua tham số
            int shift = 1; // Cần lấy thông tin ca thi từ payload hoặc qua tham số

            // Lấy danh sách kết quả từ file
            var results = _examDAO.GetAll(examDate, shift);

            var result = results.FirstOrDefault(r => r.Id == payload.ResultId);
            if (result != null)
            {
                // Cập nhật các trường
                if (payload.Similarity.HasValue)
                {
                    result.Confidence = payload.Similarity.Value;
                    result.Status = payload.Similarity.Value > 60 ? ResultStatus.MATCHED : ResultStatus.NOTMATCHED;
                }

                result.Message = "Đã hoàn thành";

                // Cập nhật lại thông tin trong file
                _examDAO.Update(examDate, shift, result);
            }
            else
            {
                // Xử lý trường hợp không tìm thấy bản ghi
                throw new KeyNotFoundException($"Result with ID {payload.ResultId} not found.");
            }
        }
    }
}
