using FAL.FrontEnd.Helper;
using FAL.FrontEnd.Models;
using FAL.FrontEnd.Service.IService;
using RestSharp;
using System.Net;

namespace FAL.FrontEnd.Service
{
    public class BaseApiService : IBaseApiService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly TokenExtentions _tokenExtentions;

        public BaseApiService(IHttpContextAccessor httpContextAccessor, TokenExtentions tokenExtentions)
        {
            _httpContextAccessor = httpContextAccessor;
            _tokenExtentions = tokenExtentions;
        }

        // Hàm gọi API có đính kèm token
        public async Task<RestResponse> CallApiAsync(
            string endpoint, Method method, 
            Dictionary<string, string>? queryParams = null,
            object? body = null)
        {
            try
            {
                // Lấy token từ Cookie
                var token = _httpContextAccessor.HttpContext?.Request.Cookies["AccessToken"];

                // **Kiểm Tra Token**
                if (string.IsNullOrEmpty(token) || !_tokenExtentions.IsTokenValid(token))
                {
                    // Token không hợp lệ -> Chuyển hướng login
                    _httpContextAccessor.HttpContext?.Response.Redirect("/Auth/login");
                    return new RestResponse
                    {
                        StatusCode = HttpStatusCode.Unauthorized,
                        Content = "Token không hợp lệ hoặc đã hết hạn. Vui lòng đăng nhập lại."
                    };
                }

                // Tạo RestSharp Client
                var client = new RestClient(FEGlobalVarians.BE_URL);

                // Tạo Request
                var request = new RestRequest(endpoint, method);

                // Đính kèm Bearer Token
                request.AddHeader("Authorization", $"Bearer {token}");

                if (queryParams != null)
                {
                    foreach (var param in queryParams)
                    {
                        request.AddQueryParameter(param.Key, param.Value);
                    }
                }

                // Nếu có dữ liệu Body thì thêm vào
                if (body != null)
                {
                    request.AddJsonBody(body);
                }

                // Gửi Request
                var response = await client.ExecuteAsync(request);

                // Kiểm tra nếu Token hết hạn (HTTP 401)
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _httpContextAccessor.HttpContext?.Response.Redirect("/Auth/login");
                }

                return response;
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
