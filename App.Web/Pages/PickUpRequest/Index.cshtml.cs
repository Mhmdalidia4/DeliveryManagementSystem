using App.Domain.DTOs;
using App.Service.Interface;
using App.Service.Managers;
using App.Web.ViewModels;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
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
        }        public async Task<IActionResult> OnGetLoadAddFormAsync()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) 
                {
                    Debug.WriteLine("ERROR: User is null");
                    return Unauthorized();
                }

                Debug.WriteLine($"User ID: {user.Id}, Email: {user.Email}");

                if (await _userManager.IsInRoleAsync(user, "company"))
                {
                    Debug.WriteLine("User is in company role");
                    
                    var companyId = await _companyManager.GetCompanyIdAsync(user);
                    Debug.WriteLine($"Company ID: {companyId}");
                    
                    var shops = await _shopManager.GetAllShopsForCompanyUserAsync(companyId);
                    var drivers = await _driverManager.GetAllDriversByCompanyAsync(companyId, user);
                   
                    Debug.WriteLine($"Shops count: {shops.Count()}");
                    Debug.WriteLine($"Drivers count: {drivers.Count()}");
                    
                    // Log shop details
                    foreach (var shop in shops)
                    {
                        Debug.WriteLine($"Shop: ID={shop.ShopId}, Name='{shop.Name}', CompanyId={shop.CompanyId}");
                    }
                    
                    // Log driver details
                    foreach (var driver in drivers)
                    {
                        Debug.WriteLine($"Driver: ID={driver.DriverId}, Name='{driver.Name}', CompanyId={driver.CompanyId}");
                    }
                    
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
                    
                    Debug.WriteLine($"ShopOptions count: {viewModel.ShopOptions.Count}");
                    Debug.WriteLine($"DriverOptions count: {viewModel.DriverOptions.Count}");
                    
                    // Log the actual select list items
                    foreach (var option in viewModel.ShopOptions)
                    {
                        Debug.WriteLine($"ShopOption: Value='{option.Value}', Text='{option.Text}'");
                    }
                    
                    foreach (var option in viewModel.DriverOptions)
                    {
                        Debug.WriteLine($"DriverOption: Value='{option.Value}', Text='{option.Text}'");
                    }
                    
                    return Partial("_AddPickUpRequestPartial", viewModel);
                }
                else
                {
                    Debug.WriteLine("User is NOT in company role");
                    var roles = await _userManager.GetRolesAsync(user);                    Debug.WriteLine($"User roles: {string.Join(", ", roles)}");
                    
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
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERROR in OnGetLoadAddFormAsync: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return BadRequest($"Error: {ex.Message}");
            }
        }

        public async Task<IActionResult> OnPostAddAsync(AddPickUpRequestViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            if (!await _userManager.IsInRoleAsync(user, "company"))
                return Forbid();

            try
            {
                if (!model.SelectedShopId.HasValue)
                {
                    ModelState.AddModelError("", "Please select a shop.");
                    return await ReloadAddFormWithErrors(model, user);
                }

                var companyId = await _companyManager.GetCompanyIdAsync(user);
                
                var pickupDto = new PickupRequestDto
                {
                    ShopId = model.SelectedShopId.Value,
                    CompanyId = companyId,
                    AssignedDriverId = model.SelectedDriverId,
                    RequestedAt = DateTime.Now,
                    IsCompleted = false
                };

                await _pickupRequestManager.AddAsync(pickupDto, user);

                return new JsonResult(new { created = true });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error creating pickup request: {ex}");
                ModelState.AddModelError("", "An error occurred while creating the pickup request.");
                return await ReloadAddFormWithErrors(model, user);
            }
        }

        private async Task<IActionResult> ReloadAddFormWithErrors(AddPickUpRequestViewModel model, IdentityUser user)
        {
            var companyId = await _companyManager.GetCompanyIdAsync(user);
            var shops = await _shopManager.GetAllShopsForCompanyUserAsync(companyId);
            var drivers = await _driverManager.GetAllDriversByCompanyAsync(companyId, user);

            model.ShopOptions = shops.Select(s => new SelectListItem
            {
                Value = s.ShopId.ToString(),
                Text = s.Name
            }).ToList();

            model.DriverOptions = drivers.Select(d => new SelectListItem
            {
                Value = d.DriverId.ToString(),
                Text = d.Name
            }).ToList();

            return Partial("_AddPickUpRequestPartial", model);
        }        public async Task<IActionResult> OnPostDeleteAsync(int id)
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
            


        }        // Debug method to test data retrieval
        public async Task<IActionResult> OnGetDebugDataAsync()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) 
                {
                    Debug.WriteLine("DEBUG: User is null");
                    return new JsonResult(new { success = false, error = "User is null" });
                }

                var roles = await _userManager.GetRolesAsync(user);
                Debug.WriteLine($"DEBUG: User ID: {user.Id}, Email: {user.Email}");
                Debug.WriteLine($"DEBUG: User roles: {string.Join(", ", roles)}");
                
                var debugInfo = new
                {
                    UserId = user.Id,
                    UserEmail = user.Email,
                    UserRoles = roles.ToArray(), // Convert to array to ensure proper serialization
                    IsCompany = roles.Contains("company")
                };

                if (roles.Contains("company"))
                {
                    Debug.WriteLine("DEBUG: User is in company role");
                    try
                    {
                        var companyId = await _companyManager.GetCompanyIdAsync(user);
                        Debug.WriteLine($"DEBUG: Company ID: {companyId}");
                        
                        var shops = await _shopManager.GetAllShopsForCompanyUserAsync(companyId);
                        var drivers = await _driverManager.GetAllDriversByCompanyAsync(companyId, user);

                        Debug.WriteLine($"DEBUG: Found {shops.Count()} shops and {drivers.Count()} drivers");

                        var shopsList = shops.Select(s => new { s.ShopId, s.Name, s.CompanyId }).ToArray();
                        var driversList = drivers.Select(d => new { d.DriverId, d.Name, d.CompanyId }).ToArray();

                        return new JsonResult(new
                        {
                            success = true,
                            user = debugInfo,
                            companyId = companyId,
                            shopsCount = shops.Count(),
                            driversCount = drivers.Count(),
                            shops = shopsList,
                            drivers = driversList
                        });
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"DEBUG ERROR in company section: {ex.Message}");
                        return new JsonResult(new
                        {
                            success = false,
                            user = debugInfo,
                            error = ex.Message,
                            stackTrace = ex.StackTrace
                        });
                    }
                }
                else
                {
                    Debug.WriteLine("DEBUG: User is NOT in company role");
                    return new JsonResult(new
                    {
                        success = true,
                        user = debugInfo,
                        message = "User is not in company role"
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"DEBUG ERROR: {ex.Message}");
                return new JsonResult(new
                {
                    success = false,
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

    }
}
