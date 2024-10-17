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

            using (var memoryStream = new MemoryStream())
            {
                // Sao chép nội dung file vào MemoryStream
                await FileName.CopyToAsync(memoryStream);

                // Lấy kết quả từ API bằng MemoryStream
                var result = await GetResultAsync(memoryStream);

                if (result != null)
                {
                    Message = "Đợi tớ xíu nghenn, check kết quả ở trang notify ạa <3";
                }
                else
                {
                    Message = "S3 lỗi rồi, kiểm tra lại đi ạ <3";
                }
            }
        }

        private async Task<string?> GetResultAsync(MemoryStream memoryStream)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            using (var form = new MultipartFormDataContent())
            {
                // Đặt vị trí của MemoryStream về đầu
                memoryStream.Position = 0;

                // Thêm UserId vào form
                form.Add(new StringContent(UserId), "UserId");

                // Tạo nội dung file từ MemoryStream
                var fileContent = new ByteArrayContent(memoryStream.ToArray());
                fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream"); // Bạn có thể thay đổi nếu cần

                // Thêm file vào nội dung của form
                form.Add(fileContent, "file", FileName.FileName);

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
