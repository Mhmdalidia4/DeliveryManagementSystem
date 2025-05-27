using System;
using System.Collections.Generic;

namespace App.Domain.Models;

public partial class Shop
{
    public int ShopId { get; set; }

    public int CompanyId { get; set; }

    public string UserId { get; set; } = null!;

    public string? Name { get; set; }

    public string? Phone { get; set; }

    public string? Address { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Company Company { get; set; } = null!;

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<OrdersToReview> OrdersToReviews { get; set; } = new List<OrdersToReview>();

    public virtual ICollection<PickUpRequest> PickupRequests { get; set; } = new List<PickUpRequest>();
}
