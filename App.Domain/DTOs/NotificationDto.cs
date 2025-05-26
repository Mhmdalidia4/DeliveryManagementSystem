using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Domain.DTOs
{
    public class NotificationDto
    {
        public int NotificationId { get; set; }
        public string? UserId { get; set; }
        public string? Message { get; set; }
        public DateTime? CreatedAt { get; set; }
        public bool? IsRead { get; set; }
    }

}
