using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Share.DTO
{
    public class UserUpdateRequest
    {
        public string? Email { get; set; }
        public string? WebhookUrl { get; set; } // URL webhook
        public string? WebhookSecretKey { get; set; } // Secret Key cho webhook
    }
}
