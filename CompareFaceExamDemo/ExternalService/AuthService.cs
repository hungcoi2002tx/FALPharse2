using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace AuthenExamCompareFaceExam.ExternalService
{
    /// <summary>
    /// Xử lý auth, lấy token
    /// </summary>
    public class AuthService
    {
        private string? token = null;
        private DateTime tokenExpiry = DateTime.MinValue;
        private readonly string _authUrl;
        private readonly string _username;
        private readonly string _password;

        // Nhận các giá trị từ bên ngoài thay vì cố định trong mã
        public AuthService(string authUrl, string username, string password)
        {
            _authUrl = authUrl;
            _username = username;
            _password = password;
        }

        private async Task<string> LoginAndGetToken()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, _authUrl);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));

            var loginContent = new StringContent(
                $"{{\"username\": \"{_username}\", \"password\": \"{_password}\"}}",
                System.Text.Encoding.UTF8,
                "application/json"
            );
            request.Content = loginContent;

            using HttpClient client = new HttpClient();
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            var json = JObject.Parse(responseBody);

            // Giả định response chứa trường 'token' và 'expires_in'
            token = json["token"]?.ToString();
            tokenExpiry = DateTime.UtcNow.AddSeconds((int)json["expiresIn"]);

            return token ?? throw new Exception("Token retrieval failed.");
        }

        public async Task<string> GetTokenAsync()
        {
            if (token == null || DateTime.UtcNow >= tokenExpiry)
            {
                token = await LoginAndGetToken();
            }
            return token;
        }
    }
}
