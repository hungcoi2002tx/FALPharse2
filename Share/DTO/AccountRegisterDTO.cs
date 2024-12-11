namespace FAL.Dtos
{
    public class AccountRegisterDTO
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string? Email { get; set; }
        public string SystemName { get; set; }
        public string? WebhookUrl { get; set; }
        public string? WebhookSecretKey { get; set; }
    }
}
