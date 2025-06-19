using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace App.Web.ViewModels
{
    public class AddPickUpRequestViewModel
    {
        [Required(ErrorMessage = "Please select a shop.")]
        [Display(Name = "Shop")]
        public int? SelectedShopId { get; set; }
        
        [Display(Name = "Driver")]
        public int? SelectedDriverId { get; set; }
        
        public List<SelectListItem> ShopOptions { get; set; } = new();
        public List<SelectListItem> DriverOptions { get; set; } = new();
    }
}
