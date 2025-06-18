using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Domain.DTOs
{
    public class OrdersToReviewDto
    {
        public int OrderToReviewId { get; set; }
        public int CompanyId { get; set; }
        public int ShopId { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public string? CustomerAddress { get; set; }
        public int? OrderAreaId { get; set; }
        public string? Note { get; set; }
        public decimal? Amount { get; set; }
        public string? TrackingPassword { get; set; }
        public bool? Status { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

}
