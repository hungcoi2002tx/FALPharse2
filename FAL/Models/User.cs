using Amazon.DynamoDBv2.DataModel;
using System.Collections.Generic;
namespace FAL.Models;

[DynamoDBTable("Users")]
public class User
{
    [DynamoDBHashKey]
    public string UserId { get; set; } // Khóa chính
    //TODO: xử lý jwt claim username hoặc userid[DynamoDBHashKey]
    public string Username { get; set; }
    public string Password { get; set; }
    public string Email { get; set; }
    public int RoleId { get; set; }  // Chỉ một RoleId duy nhất cho user, kiểu int
    public string SystemName { get; set; } // Tên hệ thống
    public string WebhookUrl { get; set; } // URL webhook để gửi phản hồi
    public string WebhookSecretKey { get; set; } // Secret Key cho webhook
    public string Status { get; set; } // Trạng thái của user
}


