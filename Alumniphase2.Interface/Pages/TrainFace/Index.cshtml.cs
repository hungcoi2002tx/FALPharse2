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
        [BindProperty]

        public string token { get; set; } = string.Empty;
        [BindProperty]

        public string Message { get; set; }
        [BindProperty]

        public string UserId { get; set; }// Thay thế bằng UserId thực tế



        public IndexModel(IWebHostEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;

        }

        public void OnGet()
        {
        }


        public async Task OnPostTrainFaceAsync()
        {


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
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };
            var _httpClient = new HttpClient(handler);
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                // Log để kiểm tra token rỗng
                Console.WriteLine("Token là null hoặc rỗng.");
            }

            // Construct the URL with UserId as a query parameter
            var requestUrl = $"{url}?userId={UserId}";

            using (var form = new MultipartFormDataContent())
            {
                memoryStream.Position = 0;

                // Tạo nội dung file từ MemoryStream
                var fileContent = new ByteArrayContent(memoryStream.ToArray());
                fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");

                // Thêm file vào request
                form.Add(fileContent, "file", FileName.FileName);

                // Send the POST request
                HttpResponseMessage response = await _httpClient.PostAsync(requestUrl, form);

                // Check the response
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
