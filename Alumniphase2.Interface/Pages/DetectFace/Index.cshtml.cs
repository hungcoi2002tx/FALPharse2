using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text;

namespace Alumniphase2.Interface.Pages.DetectFace
{
    public class IndexModel : PageModel
    {
        [BindProperty]
        public IFormFile FileName { get; set; }
        public int ImageWidth { get; private set; }
        public int ImageHeight { get; private set; }
        private readonly IWebHostEnvironment _hostingEnvironment;
        public string? ImagePath { get; private set; } = null;
        private readonly HttpClient _httpClient;



        public IndexModel(IWebHostEnvironment hostingEnvironment, HttpClient httpClient)
        {
            _hostingEnvironment = hostingEnvironment;
            _httpClient = httpClient;
        }

        public void OnGet()
        {
        }


        public async Task OnPostDetectFaceAsync() {
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
            ImagePath = "/images/" + imageFile;
            var imageBytes = System.IO.File.ReadAllBytes(imagePath);
        }


        //private async Task<string> GetResult()
        //{
        //    // Tạo HttpContent để gửi yêu cầu POST
        //    var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
        //    // Thêm header "X-Signature" với chữ ký HMAC
        //    content.Headers.Add("X-Signature", signature);

        //    var resultJson = ConvertToJson(result);
        //    var response = await _httpClient.PostAsync(webhookUrl, content);

        //    response.EnsureSuccessStatusCode();
        //    var responseContent = await response.Content.ReadAsStringAsync();
        //    await logger.LogMessageAsync("Response from API: " + responseContent);

        //    return resultJson;

        //}
    }
}
