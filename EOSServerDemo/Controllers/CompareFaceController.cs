using AuthenExamCompareFaceExam.DAO;
using AuthenExamCompareFaceExam.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;

namespace EOSServerDemo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompareFaceController : ControllerBase
    {
        private readonly string _sourceImageDirectory;
        private readonly string _dataDirectory;  // Đường dẫn lưu ảnh mục tiêu
        private readonly ExamDAO<EOSComparisonResult> _examDao;

        public CompareFaceController(IConfiguration configuration)
        {
            _sourceImageDirectory = configuration["FilePaths:SourceImageDirectory"] ?? throw new Exception("Chưa config SourceImageDirectory");
            _dataDirectory = configuration["FilePaths:DataDirectory"] ?? throw new Exception("Chưa config DataDirectory");  // Lấy cấu hình cho target image directory
            _examDao = new ExamDAO<EOSComparisonResult>(_dataDirectory);
        }

        [HttpPost("register-compare")]
        public async Task<IActionResult> RegisterCompare(string studentCode, [FromForm] IFormFile targetImage)
        {
            // Lấy đường dẫn ảnh thẻ dựa vào mã sinh viên
            string sourceImagePath = GetSourceImagePath(studentCode);

            // Kiểm tra nếu ảnh thẻ hoặc ảnh mục tiêu không tồn tại
            if (string.IsNullOrEmpty(sourceImagePath) || !System.IO.File.Exists(sourceImagePath) || targetImage == null)
            {
                return BadRequest("Both source and target images are required.");
            }

            // Tạo file tạm cho ảnh thẻ và ảnh mục tiêu
            var tempSourceImagePath = Path.GetTempFileName();
            var tempTargetImagePath = Path.GetTempFileName();

            try
            {
                // Lưu file ảnh thẻ tạm thời
                System.IO.File.Copy(sourceImagePath, tempSourceImagePath, overwrite: true);

                string examDate = DateTime.Now.ToString("yyyy-MM-dd");
                int shift = 1; // TODO: thêm logic xử lý số ca thi (theo thời gian)

                // Lưu file ảnh mục tiêu vào thư mục đã cấu hình
                string targetImagePath = Path.Combine(_dataDirectory, examDate, $"shift{shift}", "Images", $"{studentCode}.jpg");

                // Tạo thư mục nếu chưa tồn tại
                string directoryPath = Path.GetDirectoryName(targetImagePath);
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                using (var targetStream = new FileStream(targetImagePath, FileMode.Create))
                {
                    await targetImage.CopyToAsync(targetStream);
                }


                // Giả lập hàm để thêm kết quả vào danh sách chờ và gọi hàm CompareFaces để so sánh ảnh
                var resultId = await AddResultAsync(studentCode, "Chưa hoàn thành", 0.0f, "Đang so sánh ảnh, đợi trong giây lát");
                await FaceCompare.CompareFaces(tempSourceImagePath, targetImagePath, resultId);

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
            }
        }

        private async Task<int> AddResultAsync(string studentCode, string status, float confidence, string message)
        {
            var result = new EOSComparisonResult
            {
                Id = GenerateUniqueId(), // Tạo ID duy nhất cho đối tượng
                StudentCode = studentCode,
                Time = DateTime.Now,
                Status = ResultStatus.PROCESSING, // Đặt trạng thái ban đầu là đang xử lý
                Confidence = confidence,
                Note = "Đang xử lý so sánh",
                Message = message
            };

            // Ghi đối tượng vào file với ngày thi và ca thi xác định
            string examDate = DateTime.Now.ToString("yyyy-MM-dd");
            int shift = 1; // TODO: thêm logic xử lý số ca thi (theo thời gian)
            _examDao.Add(examDate, shift, result);

            return result.Id;
        }

        // Hàm phụ để tạo ID duy nhất
        private int GenerateUniqueId()
        {
            var ticksPart = (int)(DateTime.Now.Ticks % 100000000); // Lấy 8 chữ số cuối của Ticks
            var randomPart = new Random().Next(1000, 9999); // Tạo số ngẫu nhiên từ 1000 đến 9999
            return ticksPart + randomPart; // Kết hợp ticksPart và randomPart
        }

        private string GetSourceImagePath(string studentCode)
        {
            // Trả về đường dẫn file dựa trên mã sinh viên
            string filePath = Path.Combine(_sourceImageDirectory, $"{studentCode}.jpg");
            return System.IO.File.Exists(filePath) ? filePath : null;
        }
    }
}
