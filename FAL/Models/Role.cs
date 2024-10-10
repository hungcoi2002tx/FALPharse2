using Amazon.DynamoDBv2.DataModel;

namespace FAL.Models
{
    [DynamoDBTable("Roles")]
    public class Role
    {
        [DynamoDBHashKey]
        public int RoleId { get; set; }  // Thay đổi thành int
        public string RoleName { get; set; }
        public List<Permission> Permissions { get; set; }
    }

}
