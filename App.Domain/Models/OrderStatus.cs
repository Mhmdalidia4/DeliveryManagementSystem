using System;
using System.Collections.Generic;

namespace App.Domain.Models;

public partial class OrderStatus
{
    public int OrderStatusId { get; set; }

    public int CompanyId { get; set; }

    public string? Name { get; set; }

    public bool? IsFinalStatus { get; set; }

    public virtual Company Company { get; set; } = null!;

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
