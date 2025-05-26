using System;
using System.Collections.Generic;

namespace MyApp.Domain.Models;

public partial class OrdersToReview
{
    public int OrderToReviewId { get; set; }

    public int CompanyId { get; set; }

    public int ShopId { get; set; }

    public string? CustomerName { get; set; }

    public string? CustomerPhone { get; set; }

    public string? CustomerAddress { get; set; }

    public int? OrderAreaId { get; set; }

    public string? Note { get; set; }

    public decimal? Amount { get; set; }

    public string? TrackingPassword { get; set; }

    public string? ReceiptId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Company Company { get; set; } = null!;

    public virtual OrderArea? OrderArea { get; set; }

    public virtual Shop Shop { get; set; } = null!;
}
