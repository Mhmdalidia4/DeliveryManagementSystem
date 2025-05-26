using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Domain.DTOs
{
    public class SystemLogDto
    {
        public int LogId { get; set; }
        public string? Level { get; set; }
        public string? Message { get; set; }
        public string? Exception { get; set; }
        public string? Properties { get; set; }
        public DateTime Timestamp { get; set; }
    }

}
