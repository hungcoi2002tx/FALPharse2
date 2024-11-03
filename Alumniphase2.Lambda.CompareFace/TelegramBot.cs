using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace SendMessageTele;

public class TelegramBot
{
    private static readonly HttpClient client = new HttpClient();
    private static readonly string telegramApiUrl = "https://api.telegram.org/bot{0}/sendMessage";

    public static async Task SendMessageAsync(string botToken, string chatId, string message)
    {
        try
        {
            var requestUrl = string.Format(telegramApiUrl, botToken);
            var parameters = new Dictionary<string, string>
        {
            { "chat_id", chatId },
            { "text", message }
        };

            var content = new FormUrlEncodedContent(parameters);
            var response = await client.PostAsync(requestUrl, content);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Message sent successfully.");
            }
            else
            {
                Console.WriteLine("Error sending message: " + response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception occurred: " + ex.Message);
        }
    }
}