using Amazon.DynamoDBv2.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InitProject
{
    [DynamoDBTable("Roles1")]
    public class Role
    {
        [DynamoDBHashKey]
        public int RoleId { get; set; }  // Partition Key
        public string RoleName { get; set; }
        public List<Permission> Permissions { get; set; }
    }

    public class Permission
    {
        public string Resource { get; set; }
        public List<string> Actions { get; set; }
    }
}
