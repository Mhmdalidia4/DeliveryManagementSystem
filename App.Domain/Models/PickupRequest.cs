using System;
using System.Collections.Generic;

namespace MyApp.Domain.Models;

public partial class PickUpRequest
{
    public int PickupRequestId { get; set; }

    public int CompanyId { get; set; }

    public int ShopId { get; set; }

    public DateTime? RequestedAt { get; set; }

    public int? AssignedDriverId { get; set; }

    public bool? IsCompleted { get; set; }

    public virtual Driver? AssignedDriver { get; set; }

    public virtual Company Company { get; set; } = null!;

    public virtual Shop Shop { get; set; } = null!;
}
