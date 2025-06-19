using Microsoft.AspNetCore.Mvc.Rendering;

namespace App.Web.ViewModels
{
    public class AddPickUpRequestViewModel
    {
        public int? SelectedShopId { get; set; }
        public int? SelectedDriverId { get; set; }
        public List<SelectListItem> ShopOptions { get; set; } = new();
        public List<SelectListItem> DriverOptions { get; set; } = new();
    }
}
