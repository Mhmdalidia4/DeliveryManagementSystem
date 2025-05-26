using System;
using System.Collections.Generic;

namespace MyApp.Domain.Models;

public partial class OrderArea
{
    public int OrderAreaId { get; set; }

    public int CompanyId { get; set; }

    public string? Name { get; set; }

    public virtual Company Company { get; set; } = null!;

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<OrdersToReview> OrdersToReviews { get; set; } = new List<OrdersToReview>();
}
