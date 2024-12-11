using System.Text.Json.Serialization;

namespace FAL.FrontEnd.Models
{
    public class UserInfoViewDto
    {
        [JsonPropertyName("username")]
        public string UserName { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("systemName")]
        public string SystemName { get; set; }

        [JsonPropertyName("webhookUrl")]
        public string WebhookUrl { get; set; }

        [JsonPropertyName("webhookSecretKey")]
        public string WebhookSecretKey { get; set; }
    }
}
