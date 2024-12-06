using FAL.FrontEnd.Service.IService;
using Share.Model;
using System.Text.Json;
using System.Text;
using Share.Constant;
using FAL.FrontEnd.Models;
using FAL.Utils;

namespace FAL.FrontEnd.Service
{
    public class AuthService : IAuthService
    {
		private readonly IHttpClientFactory _httpClientFactory;

        public AuthService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<string> GetTokenAsync(string username, string password)
        {
			try
			{
                var client = _httpClientFactory.CreateClient("FaceDetectionAPI");
                var loginData = new { username, password };
                var content = new StringContent(JsonSerializer.Serialize(loginData), Encoding.UTF8, "application/json");

                var response = await client.PostAsync(FEGlobalVarians.LOGIN_ENDPOINT, content);
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    using JsonDocument document = JsonDocument.Parse(responseContent);

                    if (document.RootElement.TryGetProperty("token", out JsonElement tokenElement) && tokenElement.ValueKind == JsonValueKind.String)
                    {
                        return tokenElement.GetString();
                    }
                }
                return string.Empty;
            }
			catch (Exception)
			{
				throw;
			}
        }
    }
}
