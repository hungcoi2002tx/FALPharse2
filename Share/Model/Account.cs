using Amazon.DynamoDBv2.DataModel;
using System.Collections.Generic;
using System.Text.Json.Serialization;
namespace Share.Model;

[DynamoDBTable("Accounts")]
public class Account
{
    [DynamoDBHashKey]
    public string Username { get; set; }
    public string Password { get; set; }
    public string Email { get; set; }
    public int RoleId { get; set; }  // Chỉ một RoleId duy nhất cho user, kiểu int
    public string SystemName { get; set; } // Tên hệ thống
    public string WebhookUrl { get; set; } // URL webhook để gửi phản hồi
    public string WebhookSecretKey { get; set; } // Secret Key cho webhook
    public string Status { get; set; } // Trạng thái của user
}


