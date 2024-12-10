﻿using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Share.DTO
{
    public class UpdateAccountDto
    {
        [ValidateNever]
        public string? Password { get; set; }
        [ValidateNever]
        public string? Email { get; set; }
        [ValidateNever]
        public int? RoleId { get; set; } // Sử dụng nullable để kiểm tra
        [ValidateNever]
        public string? SystemName { get; set; }
        [ValidateNever]
        public string? WebhookUrl { get; set; }
        [ValidateNever]
        public string? WebhookSecretKey { get; set; }
        [ValidateNever]
        public string? Status { get; set; }
    }
}