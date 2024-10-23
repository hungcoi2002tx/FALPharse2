using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Alumniphase2.Interface.Pages.TrainFace
{
    public class IndexModel : PageModel
    {
        [BindProperty]
        public IFormFile FileName { get; set; }
        public int ImageWidth { get; private set; }
        public int ImageHeight { get; private set; }
        private readonly IWebHostEnvironment _hostingEnvironment;
        public string? FilePath { get; private set; } = null;
        string url = "http://fal-dev.eba-55qpmvbp.ap-southeast-1.elasticbeanstalk.com/api/Train/file";
        private readonly HttpClient _httpClient;
        private string token;
        public string Message { get; private set; }
        public string UserId; // Thay thế bằng UserId thực tế



        public IndexModel(IWebHostEnvironment hostingEnvironment, HttpClient httpClient)
        {
            _hostingEnvironment = hostingEnvironment;
            _httpClient = httpClient;
        }

        public void OnGet()
        {
        }


        public async Task OnPostTrainFaceAsync()
        {
            token = Request.Cookies["AuthToken"];
            if (FileName != null)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var filePath = Path.Combine(uploadsFolder, FileName.FileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await FileName.CopyToAsync(stream);
                }

                using (var image = System.Drawing.Image.FromFile(filePath))
                {
                    ImageWidth = image.Width;
                    ImageHeight = image.Height;
                }
            }

            var wwwRootPath = _hostingEnvironment.WebRootPath;
            var imageFile = FileName.FileName;
            var imagePath = Path.Combine(wwwRootPath, "images", imageFile);
            FilePath = imagePath;

           var result = await GetResultAsync();

            if (result != null)
            {
                Message = "Đợi tớ xíu nghenn, check kêt quả ở trang notify ạa <3";
            }
            else
            {
                Message = "s3 loi roi, check lại đi ạ  <3";

            }
        }

        private async Task<string?> GetResultAsync()
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);


            using (var form = new MultipartFormDataContent())
            {
                // Thêm UserId vào form
                form.Add(new StringContent(UserId), "UserId");

                // Tạo nội dung file từ đường dẫn
                var fileContent = new ByteArrayContent(System.IO.File.ReadAllBytes(FilePath));
                fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream"); // Bạn có thể thay đổi nếu cần

                // Thêm file vào nội dung của form
                form.Add(fileContent, "token", Path.GetFileName(FilePath));

                // Gửi yêu cầu POST
                HttpResponseMessage response = await _httpClient.PostAsync(url, form);

                // Kiểm tra kết quả trả về
                if (response.IsSuccessStatusCode)
                {
                    string responseData = await response.Content.ReadAsStringAsync();
                    return responseData;
                }
                else
                {
                    Console.WriteLine("Error: " + response.StatusCode);
                    return null;
                }
            }
        }
    }
}
