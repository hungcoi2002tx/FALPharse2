using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EOSServerDemo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompareFaceController : ControllerBase
    {
        [HttpPost("compare")]
        public async Task<IActionResult> Compare([FromForm] IFormFile sourceImage, [FromForm] IFormFile targetImage)
        {
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

                // Gọi hàm CompareFaces để xử lý so sánh
                var result = await FaceCompare.CompareFaces(sourceImagePath, targetImagePath);
                
                return Ok(result);
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
