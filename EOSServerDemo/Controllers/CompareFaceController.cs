using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EOSServerDemo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompareFaceController : ControllerBase
    {
        [HttpPost("register-compare")]
        public async Task<IActionResult> RegisterCompare(string studenCode, [FromForm] IFormFile targetImage)
        {
            // TODO: lấy sourceImage (ảnh thẻ) của sinh viên theo studenCode trong database
            IFormFile sourceImage = null;


            if (sourceImage == null || targetImage == null)
            {
                return BadRequest("Both source and target images are required.");
            }

            // Lưu tạm thời hai ảnh để xử lý
            var sourceImagePath = Path.GetTempFileName();
            var targetImagePath = Path.GetTempFileName();

            try
            {
                // Lưu file ảnh tạm thời
                using (var stream = new FileStream(sourceImagePath, FileMode.Create))
                {
                    await sourceImage.CopyToAsync(stream);
                }
                using (var stream = new FileStream(targetImagePath, FileMode.Create))
                {
                    await targetImage.CopyToAsync(stream);
                }

                // TODO: Gọi hàm CompareFaces để đăng ký xử lý so sánh
                await FaceCompare.CompareFaces(sourceImagePath, targetImagePath);
                // cập nhật trạng thái đang đợi kết quả cho database
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
            finally
            {
                // Xóa file tạm sau khi hoàn thành
                if (System.IO.File.Exists(sourceImagePath))
                {
                    System.IO.File.Delete(sourceImagePath);
                }
                if (System.IO.File.Exists(targetImagePath))
                {
                    System.IO.File.Delete(targetImagePath);
                }
            }
        }
    }
}
