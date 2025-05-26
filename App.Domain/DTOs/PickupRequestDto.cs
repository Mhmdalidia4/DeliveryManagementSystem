using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Domain.DTOs
{
    public class PickupRequestDto
    {
        public int PickupRequestId { get; set; }
        public int CompanyId { get; set; }
        public int ShopId { get; set; }
        public DateTime? RequestedAt { get; set; }
        public int? AssignedDriverId { get; set; }
        public bool? IsCompleted { get; set; }
    }

}
