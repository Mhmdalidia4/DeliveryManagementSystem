using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Domain.DTOs
{
    public class OrderAreaDto
    {
        public int OrderAreaId { get; set; }
        public int CompanyId { get; set; }
        public string? Name { get; set; }
    }

}
