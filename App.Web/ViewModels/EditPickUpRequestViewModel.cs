using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace App.Web.ViewModels
{    
    public class EditPickUpRequestViewModel
    {
        public int PickupRequestId { get; set; }
        
        [Display(Name = "Shop")]
        public int SelectedShopId { get; set; } // No Required attribute since it's read-only
        
        [Display(Name = "Driver")]
        public int? SelectedDriverId { get; set; }
        
        public DateTime? RequestedAt { get; set; }
        
        [Display(Name = "Status")]
        public bool IsCompleted { get; set; }
        
        public List<SelectListItem> ShopOptions { get; set; } = new();
        public List<SelectListItem> DriverOptions { get; set; } = new();
    }
}
