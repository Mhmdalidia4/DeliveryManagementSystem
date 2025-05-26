using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Domain.DTOs
{
    public class DriverDto
    {
        public int DriverId { get; set; }
        public int CompanyId { get; set; }
        public string UserId { get; set; }
        public string? Name { get; set; }
        public string? Phone { get; set; }
        public bool? IsAvailable { get; set; }
    }

}
