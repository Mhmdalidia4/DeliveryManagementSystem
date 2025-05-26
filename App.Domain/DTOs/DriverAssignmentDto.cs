using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Domain.DTOs
{
    public class DriverAssignmentDto
    {
        public int AssignmentId { get; set; }
        public int OrderId { get; set; }
        public int DriverId { get; set; }
        public DateTime? AssignedAt { get; set; }
    }

}
