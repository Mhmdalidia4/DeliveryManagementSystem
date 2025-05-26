using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Domain.DTOs
{
    public class FeedbackDto
    {
        public int FeedbackId { get; set; }
        public int OrderId { get; set; }
        public int? Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime? SubmittedAt { get; set; }
    }

}
