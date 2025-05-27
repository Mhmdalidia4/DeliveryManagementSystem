using System;
using System.Collections.Generic;

namespace App.Domain.Models;

public partial class Notification
{
    public int NotificationId { get; set; }

    public string? UserId { get; set; }

    public string? Message { get; set; }

    public DateTime? CreatedAt { get; set; }

    public bool? IsRead { get; set; }
}
