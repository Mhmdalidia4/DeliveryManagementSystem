using System;
using System.Collections.Generic;

namespace MyApp.Domain.Models;

public partial class Order
{
    public int OrderId { get; set; }

    public int CompanyId { get; set; }

    public int? ShopId { get; set; }

    public string? CustomerName { get; set; }

    public string? CustomerPhone { get; set; }

    public string? CustomerAddress { get; set; }

    public int? OrderAreaId { get; set; }

    public int? OrderStatusId { get; set; }

    public int? PaymentMethodId { get; set; }

    public string? Note { get; set; }

    public decimal? Amount { get; set; }

    public string? TrackingPassword { get; set; }

    public DateTime? EstimatedArrivalTime { get; set; }

    public bool? IsCollectedFromCustomer { get; set; }

    public bool? IsCollectedFromDriver { get; set; }

    public bool? IsPaidToShop { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Company Company { get; set; } = null!;

    public virtual ICollection<DriverAssignment> DriverAssignments { get; set; } = new List<DriverAssignment>();

    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

    public virtual OrderArea? OrderArea { get; set; }

    public virtual OrderStatus? OrderStatus { get; set; }

    public virtual PaymentMethod? PaymentMethod { get; set; }

    public virtual Shop? Shop { get; set; }
}
