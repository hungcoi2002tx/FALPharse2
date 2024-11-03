using EOSServerDemo.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EOSServerDemo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompareFaceController : ControllerBase
    {
        private readonly CompareFaceContext _faceCompare;

        public CompareFaceController(CompareFaceContext faceCompare)
        {
            _faceCompare = faceCompare;
        }

        [HttpPost("register-compare")]
        public async Task<IActionResult> RegisterCompare(string studentCode, [FromForm] IFormFile targetImage)
        {
            // Lấy đường dẫn ảnh thẻ từ cơ sở dữ liệu
            var result = await GetSourceImagePathAsync(studentCode);

            if (result == null)
            {
                return BadRequest("Khong co anh source.");
            }
            string? sourceImagePath = result.ImagePath;

            // Kiểm tra nếu ảnh thẻ không tồn tại
            if (string.IsNullOrEmpty(sourceImagePath) || !System.IO.File.Exists(sourceImagePath) || targetImage == null)
            {
                return BadRequest("Both source and target images are required.");
            }

            // Chuyển ảnh từ đường dẫn thành IFormFile
            IFormFile sourceImage = ConvertToIFormFile(sourceImagePath);

            // Tạo file tạm cho ảnh thẻ và ảnh mục tiêu
            var tempSourceImagePath = Path.GetTempFileName();
            var tempTargetImagePath = Path.GetTempFileName();

            try
            {
                // Lưu file ảnh tạm thời
                using (var sourceStream = new FileStream(tempSourceImagePath, FileMode.Create))
                {
                    await sourceImage.CopyToAsync(sourceStream);
                }
                using (var targetStream = new FileStream(tempTargetImagePath, FileMode.Create))
                {
                    await targetImage.CopyToAsync(targetStream);
                }

                var resultId = await AddResultAsync(result.SourceId, "Chưa hoàn thành", 0.0f, "Đang so sánh ảnh, đợi trong giây lát");
                // Gọi hàm CompareFaces để so sánh ảnh
                await FaceCompare.CompareFaces(tempSourceImagePath, tempTargetImagePath, resultId);

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
            finally
            {
                // Xóa file tạm sau khi hoàn thành
                if (System.IO.File.Exists(tempSourceImagePath))
                {
                    System.IO.File.Delete(tempSourceImagePath);
                }
                if (System.IO.File.Exists(tempTargetImagePath))
                {
                    System.IO.File.Delete(tempTargetImagePath);
                }
            }
        }


        private async Task<int> AddResultAsync(int sourceId, string status, float confidence, string message)
        {
            var result = new Result
            {
                SourceId = sourceId,
                Time = DateTime.Now,
                Status = status,
                Confidence = confidence,
                Message = message
            };
            await _faceCompare.Results.AddAsync(result);
            await _faceCompare.SaveChangesAsync();

            return result.ResultId;
        }

        private async Task<Source?> GetSourceImagePathAsync(string studentCode)
        {
            var result = await _faceCompare.Sources.FirstOrDefaultAsync(s => s.StudentCode == studentCode);
            return result;
        }

        private IFormFile ConvertToIFormFile(string filePath)
        {
            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            return new FormFile(fileStream, 0, fileStream.Length, "sourceImage", Path.GetFileName(filePath));
        }
    }
}
