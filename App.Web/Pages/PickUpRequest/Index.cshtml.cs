using App.Domain.DTOs;
using App.Service.Interface;
using App.Service.Managers;
using App.Web.ViewModels;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Diagnostics;
namespace App.Web.pages.PickUpRequest
{
    public class IndexModel : PageModel
    {
        private readonly IPickUpRequestManager _pickupRequestManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IMapper _mapper;
        private readonly ICompanyManager _companyManager;
        private readonly IShopManager _shopManager;
        private readonly IDriverManager _driverManager;
        public IndexModel(
            IPickUpRequestManager pickupRequestManager,
            UserManager<IdentityUser> userManager,
            IMapper mapper,
            ICompanyManager companyManager,
            IShopManager shopManager,
            IDriverManager driverManager
            )
        {
            _pickupRequestManager = pickupRequestManager;
            _userManager = userManager;
            _mapper = mapper;
            _companyManager = companyManager;
            _shopManager = shopManager;
            _driverManager = driverManager;
        }

        public List<PickUpRequestViewModel> PickupRequests { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            ViewData["Title"] = "Pickup Requests";

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return RedirectToPage("/Account/Login");

            var pickupDtos = await _pickupRequestManager.GetAllByUserAsync(currentUser);
            if (pickupDtos == null || !pickupDtos.Any())
            {
                ViewData["Message"] = "No pickup requests found.";
                return Page(); // ✅ Explicitly return the page
            }

            PickupRequests = _mapper.Map<List<PickUpRequestViewModel>>(pickupDtos);

            return Page(); // ✅ Explicitly return the page
        }

        public async Task<IActionResult> OnGetLoadAddFormAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            if (await _userManager.IsInRoleAsync(user, "company"))
            {
                var companyId = await _companyManager.GetCompanyIdAsync(user);
                var shops = await _shopManager.GetAllShopsForCompanyUserAsync(companyId);
                var drivers = await _driverManager.GetAllDriversByCompanyAsync(companyId, user);
               
                var viewModel = new AddPickUpRequestViewModel
                {

                    ShopOptions = shops.Select(s => new SelectListItem
                    {

                        Value = s.ShopId.ToString(),
                        Text = s.Name
                    }).ToList(),
                    DriverOptions = drivers.Select(d => new SelectListItem
                    {
                        Value = d.DriverId.ToString(),
                        Text = d.Name
                    }).ToList()
                };
                
                Debug.WriteLine("Shops count: " + shops.Count());
                Debug.WriteLine("Drivers count: " + drivers.Count());
                return Partial("_AddPickUpRequestPartial", viewModel);
            }

            if (await _userManager.IsInRoleAsync(user, "shop"))
            {
                var shopId = await _shopManager.GetShopIdByUserAsync(user);
                var shop = await _shopManager.GetShopEntityByIdAsync(shopId);
                if (shop == null)
                    return BadRequest("Shop not found");

                var pickupDto = new PickupRequestDto
                {
                    ShopId = shop.ShopId,
                    CompanyId = shop.CompanyId,
                    RequestedAt = DateTime.Now,
                    IsCompleted = false
                };

                await _pickupRequestManager.AddAsync(pickupDto, user);

                return new JsonResult(new { created = true });
            }

            return Forbid();
        }


        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> OnPostDeleteAsync(int id)
            {
                try
                {
                    var user = await _userManager.GetUserAsync(User);
                    if (user == null) return Unauthorized();

                    var roles = await _userManager.GetRolesAsync(user);
                    bool isCompany = roles.Contains("company");
                    bool isShop = roles.Contains("shop");

                    // Get the pickup request
                    var pickupRequests = await _pickupRequestManager.GetAllByUserAsync(user);
                    var pickup = pickupRequests.FirstOrDefault(r => r.PickupRequestId == id);
               
                if (pickup == null)
                        return new JsonResult(new { success = false, message = "Pickup request not found." });

                    if (isShop && pickup.IsCompleted == true)
                        return new JsonResult(new { success = false, message = "You cannot delete a completed pickup request." });

                    // Company can always delete, shop can delete if not completed
                    await _pickupRequestManager.DeleteAsync(id, user);
                    return new JsonResult(new { success = true });
                }
                catch (Exception ex)
                {
                Debug.WriteLine($"Error deleting pickup request: {ex}");

                return new JsonResult(new { success = false, message = ex.Message });
                }
            


        }


    }
}
