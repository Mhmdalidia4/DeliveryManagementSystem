using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Domain.DTOs
{
    public class CompanyDto
    {
        public int CompanyId { get; set; }
        public string UserId { get; set; }
        public string? Name { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public DateOnly? LicenseStartDate { get; set; }
        public DateOnly? LicenseEndDate { get; set; }
        public string? SubscriptionType { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

}
