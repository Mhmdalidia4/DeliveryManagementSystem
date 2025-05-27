using System;
using System.Collections.Generic;

namespace App.Domain.Models;

public partial class SystemLog
{
    public int LogId { get; set; }

    public string? Level { get; set; }

    public string? Message { get; set; }

    public string? Exception { get; set; }

    public string? Properties { get; set; }

    public DateTime Timestamp { get; set; }
}
