using DemoSendWebhook;
using System.Security.Cryptography;
using System.Text;


//string secretKey = "your-secret-key";  // Key lấy từ appsettings.json
//string payload = "{\"imageId\":\"IMG123456\",\"systemName\":\"ImageRecognitionSystem\",\"users\":[{\"userId\":\"USER001\",\"userName\":\"Alice\"},{\"userId\":\"USER002\",\"userName\":\"Bob\"}],\"timestamp\":\"2024-10-03T12:45:00Z\"}";

//string hmacSignature = GenerateHMAC(payload, secretKey);
//Console.WriteLine($"Signature: {hmacSignature}");
await WebhookClient.SendWebhookAsync();


static string GenerateHMAC(string payload, string secret)
{
    using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret)))
    {
        var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        return Convert.ToBase64String(hashBytes);
    }
}