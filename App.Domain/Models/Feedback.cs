using System;
using System.Collections.Generic;

namespace MyApp.Domain.Models;

public partial class Feedback
{
    public int FeedbackId { get; set; }

    public int OrderId { get; set; }

    public int? Rating { get; set; }

    public string? Comment { get; set; }

    public DateTime? SubmittedAt { get; set; }

    public virtual Order Order { get; set; } = null!;
}
