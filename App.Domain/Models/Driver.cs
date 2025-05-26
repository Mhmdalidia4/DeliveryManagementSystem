using System;
using System.Collections.Generic;

namespace MyApp.Domain.Models;

public partial class Driver
{
    public int DriverId { get; set; }

    public int CompanyId { get; set; }

    public string UserId { get; set; } = null!;

    public string? Name { get; set; }

    public string? Phone { get; set; }

    public bool? IsAvailable { get; set; }

    public virtual Company Company { get; set; } = null!;

    public virtual ICollection<DriverAssignment> DriverAssignments { get; set; } = new List<DriverAssignment>();

    public virtual ICollection<PickupRequest> PickupRequests { get; set; } = new List<PickupRequest>();
}
