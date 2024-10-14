namespace FALWebhook.Models
{
    public class WebhookPayload
    {
        public string? ImageId { get; set; }  // ID của bức ảnh
        public string? SystemName { get; set; }  // Tên của hệ thống gửi dữ liệu
        public List<UserInfo>? Users { get; set; }  // Danh sách người dùng có trong ảnh
        public DateTime? Timestamp { get; set; }  // Thời gian xảy ra sự kiện
    }
}
