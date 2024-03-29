﻿using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ISPSS.Models
{
    public class Subdomain
    {
        [Required]
        [DisplayName("Subdomain")]
        public string domain { get; set; }
        public string IdentityTenantId { get; set; }
        public string AwsRegion { get; set; }
        public string IdentityPodId { get; set; }
    }
}
