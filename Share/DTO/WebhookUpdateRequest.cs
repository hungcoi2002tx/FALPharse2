using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Share.DTO
{
    public class WebhookUpdateRequest
    {
        public string WebhookUrl { get; set; }
        public string WebhookSecretKey { get; set; }
    }
}
