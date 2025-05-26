using System;
using System.Collections.Generic;

namespace MyApp.Domain.Models;

public partial class DriverAssignment
{
    public int AssignmentId { get; set; }

    public int OrderId { get; set; }

    public int DriverId { get; set; }

    public DateTime? AssignedAt { get; set; }

    public virtual Driver Driver { get; set; } = null!;

    public virtual Order Order { get; set; } = null!;
}
