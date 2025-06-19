namespace App.Web.ViewModels
{    public class PickUpRequestViewModel
    {
        public int PickupRequestId { get; set; }
        public DateTime? RequestedAt { get; set; }
        public bool? IsCompleted { get; set; }
        public int? AssignedDriverId { get; set; }

        // These are display properties joined from other models:
        public string? ShopName { get; set; }
        public string? DriverName { get; set; }
    }
}
