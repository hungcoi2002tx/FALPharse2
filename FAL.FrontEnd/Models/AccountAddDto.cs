﻿using System.Text.Json.Serialization;

namespace FAL.FrontEnd.Models
{
    public class AccountAddDto
    {
        [JsonPropertyName("username")]
        public string Username { get; set; }
        [JsonPropertyName("password")]
        public string Password { get; set; }

        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("roleId")]
        public int RoleId { get; set; }

        [JsonPropertyName("systemName")]
        public string SystemName { get; set; }

        [JsonPropertyName("webhookUrl")]
        public string? WebhookUrl { get; set; }

        [JsonPropertyName("webhookSecretKey")]
        public string? WebhookSecretKey { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }
    }
}
