using EOSServerDemo.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography.Xml;

namespace EOSServerDemo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WebhookReceiverCompareFaceController : ControllerBase
    {
        private readonly CompareFaceContext _compareFace;

        public WebhookReceiverCompareFaceController(CompareFaceContext compareFace)
        {
            _compareFace = compareFace;
        }

        [HttpPost("ReceiveData")]
        public async Task<IActionResult> ReceiveData([FromBody] ComparisonResult payload)
        {
            try
            {
                await UpdateResultAsync(payload);
                var payloadString = System.Text.Json.JsonSerializer.Serialize(payload);
                return Ok(payloadString);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
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
                result.Status = "Đã hoàn thành";
                if (payload.Similarity.HasValue)
                {
                    result.Confidence = (float)payload.Similarity.Value;
                }
                result.Message = payload.SourceImageUrl + "|" +
                    "" +
                    "" +
                    "" + payload.TargetImageUrl;
                result.Time = DateTime.Now;

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
