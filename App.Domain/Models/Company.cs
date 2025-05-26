using System;
using System.Collections.Generic;

namespace MyApp.Domain.Models;

public partial class Company
{
    public int CompanyId { get; set; }

    public string UserId { get; set; } = null!;

    public string? Name { get; set; }

    public string? Phone { get; set; }

    public string? Address { get; set; }

    public DateOnly? LicenseStartDate { get; set; }

    public DateOnly? LicenseEndDate { get; set; }

    public string? SubscriptionType { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Driver> Drivers { get; set; } = new List<Driver>();

    public virtual ICollection<OrderArea> OrderAreas { get; set; } = new List<OrderArea>();

    public virtual ICollection<OrderStatus> OrderStatuses { get; set; } = new List<OrderStatus>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<OrdersToReview> OrdersToReviews { get; set; } = new List<OrdersToReview>();

    public virtual ICollection<PaymentMethod> PaymentMethods { get; set; } = new List<PaymentMethod>();

    public virtual ICollection<PickUpRequest> PickupRequests { get; set; } = new List<PickUpRequest>();

    public virtual ICollection<Shop> Shops { get; set; } = new List<Shop>();
}
