using App.Service.Managers;
using App.Web.ViewModels;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using App.Service.Interface;
namespace App.Web.pages.PickUpRequest
{
    public class IndexModel : PageModel
    {
        private readonly IPickUpRequestManager _pickupRequestManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IMapper _mapper;

        public IndexModel(
            IPickUpRequestManager pickupRequestManager,
            UserManager<IdentityUser> userManager,
            IMapper mapper)
        {
            _pickupRequestManager = pickupRequestManager;
            _userManager = userManager;
            _mapper = mapper;
        }

        public List<PickupRequestViewModel> PickupRequests { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            ViewData["Title"] = "Pickup Requests";

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return RedirectToPage("/Account/Login");

            var pickupDtos = await _pickupRequestManager.GetAllByUserAsync(currentUser);
            PickupRequests = _mapper.Map<List<PickupRequestViewModel>>(pickupDtos);

            return Page(); // ✅ Explicitly return the page
        }
    }
}
