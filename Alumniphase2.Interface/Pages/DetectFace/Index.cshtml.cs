﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Headers;
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
        public string? FilePath { get; private set; } = null;
        string url = "http://fal-dev.eba-55qpmvbp.ap-southeast-1.elasticbeanstalk.com/api/Detect";
        private readonly HttpClient _httpClient;
        private string token;
        public string Message;

        //public string Token { get; private set; }



        public IndexModel(IWebHostEnvironment hostingEnvironment, HttpClient httpClient)
        {
            _hostingEnvironment = hostingEnvironment;
            _httpClient = httpClient;
        }

        public void OnGet()
        {


        }


        public async Task OnPostDetectFaceAsync()
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
                Message = "Hưng ơi s3 lỗi cụ m r @@";
            }
        }

        private async Task<string?> GetResultAsync()
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            using (var form = new MultipartFormDataContent())
            {
                // Tạo nội dung file
                var fileContent = new ByteArrayContent(System.IO.File.ReadAllBytes(FilePath));
                fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");

                // Thêm file vào request
                form.Add(fileContent, "file", Path.GetFileName(FilePath));

                // Gửi request POST
                HttpResponseMessage response = await _httpClient.PostAsync(url, form);

                // Kiểm tra kết quả
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
