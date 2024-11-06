using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EOSServerDemo;

public class FaceCompare
{
    private static string? token = null;
    private static DateTime tokenExpiry = DateTime.MinValue;

    private static async Task<string> LoginAndGetToken()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "https://localhost:7065/api/Auth/login");
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
        var loginContent = new StringContent("{\"username\": \"khanm\", \"password\": \"123456\"}", System.Text.Encoding.UTF8, "application/json");
        request.Content = loginContent;
    using HttpClient client = new HttpClient();

    var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadAsStringAsync();
        var json = JObject.Parse(responseBody);

        // Assuming the response contains token and expiry fields
        token = json["token"].ToString();
        tokenExpiry = DateTime.UtcNow.AddSeconds((int)json["expires_in"]);

        return token;
    }

    private static async Task<string> GetToken()
    {
        if (token == null || DateTime.UtcNow >= tokenExpiry)
        {
            token = await LoginAndGetToken();
        }
        return token;
    }

    public static async Task<bool> CompareFaces(string sourceImagePath, string targetImagePath, int sourceId)
    {
        try
        {
            string token = await GetToken();

            var request = new HttpRequestMessage(HttpMethod.Post, "https://localhost:7065/api/Compare/compare");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));

            var content = new MultipartFormDataContent();
            content.Add(new StreamContent(File.OpenRead(sourceImagePath)), "SourceImage", Path.GetFileName(sourceImagePath));
            content.Add(new StreamContent(File.OpenRead(targetImagePath)), "TargetImage", Path.GetFileName(targetImagePath));
            content.Add(new StringContent(sourceId.ToString()), "SourceId");

            request.Content = content;
            using HttpClient client = new HttpClient();

            var response = await client.SendAsync(request);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                // If token is expired, get new token and retry
                token = await LoginAndGetToken();
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                response = await client.SendAsync(request);
            }

            response.EnsureSuccessStatusCode();

        }
        catch (Exception)
        {
            return false;
        }
            return true;
    }
}