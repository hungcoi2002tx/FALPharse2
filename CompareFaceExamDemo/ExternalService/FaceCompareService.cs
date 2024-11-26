using CompareFaceExamDemo.Models;
using Newtonsoft.Json;
using Share.SystemModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace CompareFaceExamDemo.ExternalService
{
    /// <summary>
    /// Xử lý call api bên FAL để nhận về data
    /// </summary>
    public class FaceCompareService
    {
        private readonly AuthService _authService;
        private readonly string _compareUrl;

        public FaceCompareService(AuthService authService, string compareUrl)
        {
            _authService = authService;
            _compareUrl = compareUrl;
        }

        public async Task<ComparisonResponse> CompareFacesAsync(string sourceImagePath, string targetImagePath)
        {
            try
            {
                string token = await _authService.GetTokenAsync();

                var request = new HttpRequestMessage(HttpMethod.Post, _compareUrl);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));

                var content = new MultipartFormDataContent
                {
                    { new StreamContent(File.OpenRead(sourceImagePath)), "SourceImage", Path.GetFileName(sourceImagePath) },
                    { new StreamContent(File.OpenRead(targetImagePath)), "TargetImage", Path.GetFileName(targetImagePath) }
                };

                request.Content = content;
                using HttpClient client = new HttpClient();

                var response = await client.SendAsync(request);

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    // Nếu token hết hạn, lấy token mới và thử lại
                    token = await _authService.GetTokenAsync();
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    response = await client.SendAsync(request);
                }

                // Đọc phản hồi từ API
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    // Parse dữ liệu JSON từ phản hồi
                    var resultData = JsonConvert.DeserializeObject<CompareResponseResult>(responseContent);
                    return new ComparisonResponse
                    {
                        Status = (int)response.StatusCode,
                        Message = "Comparison successful.",
                        Data = resultData
                    };
                }
                else
                {
                    // Trả về khi có lỗi với thông báo từ server
                    return new ComparisonResponse
                    {
                        Status = (int)response.StatusCode,
                        Message = $"Error: {responseContent}",
                        Data = null
                    };
                }
            }
            catch (Exception ex)
            {
                // Trả về trong trường hợp có ngoại lệ
                return new ComparisonResponse
                {
                    Status = 500,
                    Message = $"Exception occurred: {ex.Message}",
                    Data = null
                };
            }
        }
    }
}
