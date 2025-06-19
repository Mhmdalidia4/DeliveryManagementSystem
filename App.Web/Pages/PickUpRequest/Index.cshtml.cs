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

        [BindProperty]
        public AddPickUpRequestViewModel AddPickUpRequestModel { get; set; } = new();

        [BindProperty]
        public EditPickUpRequestViewModel EditPickUpRequestModel { get; set; } = new();        public async Task<IActionResult> OnGetAsync()
        {
            ViewData["Title"] = "Pickup Requests";

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return RedirectToPage("/Account/Login");

            // Get driver ID if user is a driver
            var isDriver = await _userManager.IsInRoleAsync(currentUser, "driver");
            if (isDriver)
            {
                var driverRecord = await _driverManager.GetDriverByUserIdAsync(currentUser.Id);
                ViewData["CurrentDriverId"] = driverRecord?.DriverId;
            }

            var pickupDtos = await _pickupRequestManager.GetAllByUserAsync(currentUser);
            if (pickupDtos == null || !pickupDtos.Any())
            {
                ViewData["Message"] = "No pickup requests found.";
                return Page(); // ✅ Explicitly return the page
            }

            PickupRequests = _mapper.Map<List<PickUpRequestViewModel>>(pickupDtos);

            return Page(); // ✅ Explicitly return the page
        }public async Task<IActionResult> OnGetLoadAddFormAsync()
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
        }        public async Task<IActionResult> OnPostAddAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            if (!await _userManager.IsInRoleAsync(user, "company"))
                return Forbid();

            try
            {
                if (!AddPickUpRequestModel.SelectedShopId.HasValue)
                {
                    ModelState.AddModelError("", "Please select a shop.");
                    return await ReloadAddFormWithErrors(AddPickUpRequestModel, user);
                }

                var companyId = await _companyManager.GetCompanyIdAsync(user);
                
                var pickupDto = new PickupRequestDto
                {
                    ShopId = AddPickUpRequestModel.SelectedShopId.Value,
                    CompanyId = companyId,
                    AssignedDriverId = AddPickUpRequestModel.SelectedDriverId,
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
                return await ReloadAddFormWithErrors(AddPickUpRequestModel, user);
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
        }        public async Task<IActionResult> OnGetLoadEditFormAsync(int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Unauthorized();                var userRoles = await _userManager.GetRolesAsync(user);
                
                // More detailed debugging
                System.Diagnostics.Debug.WriteLine($"DEBUG EDIT: User ID: {user.Id}");
                System.Diagnostics.Debug.WriteLine($"DEBUG EDIT: User Email: {user.Email}");
                System.Diagnostics.Debug.WriteLine($"DEBUG EDIT: UserRoles Count: {userRoles.Count}");
                System.Diagnostics.Debug.WriteLine($"DEBUG EDIT: UserRoles: [{string.Join(", ", userRoles)}]");
                
                // Try multiple ways to check roles
                var isCompany1 = userRoles.Any(r => r.Equals("company", StringComparison.OrdinalIgnoreCase));
                var isDriver1 = userRoles.Any(r => r.Equals("driver", StringComparison.OrdinalIgnoreCase));
                var isCompany2 = userRoles.Contains("company");
                var isDriver2 = userRoles.Contains("driver");
                var isCompany3 = await _userManager.IsInRoleAsync(user, "company");
                var isDriver3 = await _userManager.IsInRoleAsync(user, "driver");
                
                System.Diagnostics.Debug.WriteLine($"DEBUG EDIT: IsCompany (Any/OrdinalIgnoreCase): {isCompany1}");
                System.Diagnostics.Debug.WriteLine($"DEBUG EDIT: IsDriver (Any/OrdinalIgnoreCase): {isDriver1}");
                System.Diagnostics.Debug.WriteLine($"DEBUG EDIT: IsCompany (Contains): {isCompany2}");
                System.Diagnostics.Debug.WriteLine($"DEBUG EDIT: IsDriver (Contains): {isDriver2}");
                System.Diagnostics.Debug.WriteLine($"DEBUG EDIT: IsCompany (IsInRoleAsync): {isCompany3}");
                System.Diagnostics.Debug.WriteLine($"DEBUG EDIT: IsDriver (IsInRoleAsync): {isDriver3}");
                
                // Use the most reliable method
                var isCompany = isCompany3;
                var isDriver = isDriver3;
                
                if (!isCompany && !isDriver)
                    return Forbid();

                // Get the pickup request
                var pickupRequests = await _pickupRequestManager.GetAllByUserAsync(user);
                var pickup = pickupRequests.FirstOrDefault(r => r.PickupRequestId == id);
                
                if (pickup == null)
                    return NotFound();

                // For drivers, additional validation - they can only edit requests assigned to them
                if (isDriver)
                {
                    if (pickup.AssignedDriverId == null)
                        return Forbid(); // Not assigned to any driver
                        
                    var driverRecord = await _driverManager.GetDriverByUserIdAsync(user.Id);
                    if (driverRecord == null || pickup.AssignedDriverId != driverRecord.DriverId)
                        return Forbid(); // Not assigned to this driver
                }

                var viewModel = new EditPickUpRequestViewModel
                {
                    PickupRequestId = pickup.PickupRequestId,
                    SelectedShopId = pickup.ShopId,
                    SelectedDriverId = pickup.AssignedDriverId,
                    RequestedAt = pickup.RequestedAt,
                    IsCompleted = pickup.IsCompleted ?? false
                };

                // Only populate driver options for company users
                if (isCompany)
                {
                    var companyId = await _companyManager.GetCompanyIdAsync(user);
                    var shops = await _shopManager.GetAllShopsForCompanyUserAsync(companyId);
                    var drivers = await _driverManager.GetAllDriversByCompanyAsync(companyId, user);

                    viewModel.ShopOptions = shops.Select(s => new SelectListItem
                    {
                        Value = s.ShopId.ToString(),
                        Text = s.Name
                    }).ToList();

                    viewModel.DriverOptions = drivers.Select(d => new SelectListItem
                    {
                        Value = d.DriverId.ToString(),
                        Text = d.Name
                    }).ToList();
                }                else // isDriver
                {
                    var pickupShop = await _shopManager.GetShopEntityByIdAsync(pickup.ShopId);
                    
                    viewModel.ShopOptions = new List<SelectListItem>
                    {
                        new SelectListItem { Value = pickup.ShopId.ToString(), Text = pickupShop?.Name ?? "Unknown Shop" }
                    };
                    viewModel.DriverOptions = new List<SelectListItem>();
                }                ViewData["IsCompany"] = isCompany;
                ViewData["IsDriver"] = isDriver;
                
                System.Diagnostics.Debug.WriteLine($"DEBUG EDIT: Setting ViewData - IsCompany: {isCompany}, IsDriver: {isDriver}");
                
                return Partial("_EditPickUpRequestPartial", viewModel);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }        public async Task<IActionResult> OnPostEditAsync()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Unauthorized();

                var userRoles = await _userManager.GetRolesAsync(user);
                var isCompany = userRoles.Any(r => r.Equals("company", StringComparison.OrdinalIgnoreCase));
                var isDriver = userRoles.Any(r => r.Equals("driver", StringComparison.OrdinalIgnoreCase));
                  if (!isCompany && !isDriver)
                    return Forbid();                // Remove validation errors for shop selection since it's read-only in edit
                if (ModelState.ContainsKey("EditPickUpRequestModel.SelectedShopId"))
                {
                    ModelState.Remove("EditPickUpRequestModel.SelectedShopId");
                }                // Additional validation clearing for any shop-related errors
                var keysToRemove = ModelState.Keys.Where(k => k.Contains("Shop")).ToList();
                foreach (var key in keysToRemove)
                {
                    ModelState.Remove(key);
                }

                // Debug: Log the EditPickUpRequestModel values
                System.Diagnostics.Debug.WriteLine($"EDIT DEBUG: PickupRequestId = {EditPickUpRequestModel?.PickupRequestId}");
                System.Diagnostics.Debug.WriteLine($"EDIT DEBUG: SelectedShopId = {EditPickUpRequestModel?.SelectedShopId}");
                System.Diagnostics.Debug.WriteLine($"EDIT DEBUG: SelectedDriverId = {EditPickUpRequestModel?.SelectedDriverId}");
                System.Diagnostics.Debug.WriteLine($"EDIT DEBUG: IsCompleted = {EditPickUpRequestModel?.IsCompleted}");

                if (!ModelState.IsValid)
                {
                    System.Diagnostics.Debug.WriteLine("EDIT DEBUG: ModelState is invalid:");
                    foreach (var modelError in ModelState)
                    {
                        foreach (var error in modelError.Value.Errors)
                        {
                            System.Diagnostics.Debug.WriteLine($"EDIT DEBUG: {modelError.Key}: {error.ErrorMessage}");
                        }
                    }
                    return await ReloadEditFormWithErrors(EditPickUpRequestModel ?? new EditPickUpRequestViewModel(), user);
                }

                if (EditPickUpRequestModel == null)
                {
                    ModelState.AddModelError("", "Invalid request data.");
                    return await ReloadEditFormWithErrors(new EditPickUpRequestViewModel(), user);
                }

                var pickupRequests = await _pickupRequestManager.GetAllByUserAsync(user);
                var existingPickup = pickupRequests.FirstOrDefault(r => r.PickupRequestId == EditPickUpRequestModel.PickupRequestId);
                
                if (existingPickup == null)
                    return NotFound();// For drivers, additional validation - they can only edit requests assigned to them
                if (isDriver)
                {                    if (existingPickup.AssignedDriverId == null)
                    {
                        ModelState.AddModelError("", "This pickup request is not assigned to any driver.");
                        return await ReloadEditFormWithErrors(EditPickUpRequestModel, user);
                    }
                    
                    // Get the driver record to verify the user is the assigned driver
                    var driverRecord = await _driverManager.GetDriverByUserIdAsync(user.Id);                    if (driverRecord == null || existingPickup.AssignedDriverId != driverRecord.DriverId)
                    {
                        ModelState.AddModelError("", "You can only edit pickup requests assigned to you.");
                        return await ReloadEditFormWithErrors(EditPickUpRequestModel, user);
                    }
                    
                    // Drivers can only change status from pending to completed, not back to pending
                    if (existingPickup.IsCompleted == true && EditPickUpRequestModel.IsCompleted == false)
                    {
                        ModelState.AddModelError("", "You cannot change the status back to pending once it's completed.");
                        return await ReloadEditFormWithErrors(EditPickUpRequestModel, user);
                    }
                }                // Create update DTO with different logic for company vs driver
                PickupRequestDto updateDto;
                if (isCompany)
                {
                    // Company can update driver assignment and status
                    updateDto = new PickupRequestDto
                    {
                        PickupRequestId = EditPickUpRequestModel.PickupRequestId,
                        ShopId = existingPickup.ShopId, // Keep original shop
                        CompanyId = existingPickup.CompanyId,
                        AssignedDriverId = EditPickUpRequestModel.SelectedDriverId, // Can be null to remove driver
                        RequestedAt = existingPickup.RequestedAt, // Keep original date
                        IsCompleted = EditPickUpRequestModel.IsCompleted // Update completion status
                    };
                }else // isDriver
                {
                    // Driver can only update status, keep everything else the same
                    updateDto = new PickupRequestDto
                    {
                        PickupRequestId = EditPickUpRequestModel.PickupRequestId,
                        ShopId = existingPickup.ShopId, // Keep original shop
                        CompanyId = existingPickup.CompanyId,
                        AssignedDriverId = existingPickup.AssignedDriverId, // Keep original driver assignment
                        RequestedAt = existingPickup.RequestedAt, // Keep original date
                        IsCompleted = EditPickUpRequestModel.IsCompleted // Only update completion status
                    };
                }

                await _pickupRequestManager.UpdateAsync(updateDto, user);

                return new JsonResult(new { updated = true });
            }            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating pickup request: {ex}");
                ModelState.AddModelError("", "An error occurred while updating the pickup request.");
                var userForReload = await _userManager.GetUserAsync(User);
                if (userForReload == null) return Unauthorized();
                return await ReloadEditFormWithErrors(EditPickUpRequestModel ?? new EditPickUpRequestViewModel(), userForReload);
            }
        }        private async Task<IActionResult> ReloadEditFormWithErrors(EditPickUpRequestViewModel model, IdentityUser user)
        {
            // Get user roles for consistency
            var userRoles = await _userManager.GetRolesAsync(user);
            var isCompany = userRoles.Any(r => r.Equals("company", StringComparison.OrdinalIgnoreCase));
            var isDriver = userRoles.Any(r => r.Equals("driver", StringComparison.OrdinalIgnoreCase));

            // Only populate dropdown options for company users
            if (isCompany)
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
            }            else
            {
                // For drivers, get shop information for display
                var pickupShop = await _shopManager.GetShopEntityByIdAsync(model.SelectedShopId);
                
                model.ShopOptions = new List<SelectListItem>
                {
                    new SelectListItem { Value = model.SelectedShopId.ToString(), Text = pickupShop?.Name ?? "Unknown Shop" }
                };
                model.DriverOptions = new List<SelectListItem>();
            }

            // Add role information to ViewData so the partial can use it
            ViewData["IsCompany"] = isCompany;
            ViewData["IsDriver"] = isDriver;

            return Partial("_EditPickUpRequestPartial", model);
        }
    }
}
