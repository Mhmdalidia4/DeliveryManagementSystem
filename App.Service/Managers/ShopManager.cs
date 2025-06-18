using App.Domain.DTOs;
using App.Domain.Interfaces;
using App.Domain.Interfaces.Base;
using App.Domain.Models;
using App.Service.Interface;
using AutoMapper;
using Microsoft.AspNetCore.Identity;

namespace App.Service.Managers
{
    public class ShopManager: IShopManager
    {
        private readonly IBaseRepository<Shop> _shopRepo;
        private readonly IBaseRepository<Company> _companyRepo;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IMapper _mapper;

        public ShopManager(
            IBaseRepository<Shop> shopRepo,
            IBaseRepository<Company> companyRepo,
            UserManager<IdentityUser> userManager,
            IMapper mapper)
        {
            _shopRepo = shopRepo;
            _companyRepo = companyRepo;
            _userManager = userManager;
            _mapper = mapper;
        }

        // 1. Get all shops belong to a company (only company owner)
        public async Task<IEnumerable<ShopDto>> GetAllShopsForCurrentCompanyUserAsync(IdentityUser currentUser)
        {
            // Ensure user has company role
            if (!await _userManager.IsInRoleAsync(currentUser, "company"))
                throw new UnauthorizedAccessException("Not authorized. Only company users can view their shops.");

            // Find the company where this user is the owner
            var company = (await _companyRepo.FindAsync(c => c.UserId == currentUser.Id)).FirstOrDefault();
            if (company == null)
                throw new KeyNotFoundException("No company found for this user.");

            // Get all shops for this company
            var shops = await _shopRepo.FindAsync(s => s.CompanyId == company.CompanyId);
            return _mapper.Map<IEnumerable<ShopDto>>(shops);
        }



        // 2. Add shop (create IdentityUser, then Shop; only company owner can add)
        public async Task<ShopDto> AddShopAsync(
    ShopDto shopDto,
    string shopEmail,
    string shopPassword,
    IdentityUser currentUser)
{
    // Only company owner can create a shop
    var company = await _companyRepo.GetByIdAsync(shopDto.CompanyId);
    if (company == null || company.UserId != currentUser.Id)
        throw new UnauthorizedAccessException("Not authorized to create a shop for this company.");

    // 1. Create IdentityUser for shop
    var identityUser = new IdentityUser
    {
        UserName = shopEmail,
        Email = shopEmail
    };
    var result = await _userManager.CreateAsync(identityUser, shopPassword);
    if (!result.Succeeded)
        throw new Exception($"Failed to create shop user: {string.Join(", ", result.Errors.Select(e => e.Description))}");

    // Optionally add to 'shop' role
    await _userManager.AddToRoleAsync(identityUser, "shop");

    // 2. Map and add Shop entity
    var shop = _mapper.Map<Shop>(shopDto);
    shop.UserId = identityUser.Id; // Reference to new IdentityUser
    shop.CreatedAt = DateTime.Now;

    await _shopRepo.AddAsync(shop);

    // Return the created ShopDto (with new ShopId)
    return _mapper.Map<ShopDto>(shop);
}


        // 3. Edit shop (only company owner or shop itself)
        public async Task EditShopAsync(ShopDto shopDto, IdentityUser currentUser)
        {
            var shop = await _shopRepo.GetByIdAsync(shopDto.ShopId);
            var company = await _companyRepo.GetByIdAsync(shop.CompanyId);
            // Only company owner or the shop user themself can edit
            bool isCompanyOwner = company != null && company.UserId == currentUser.Id;
            bool isShopUser = shop.UserId == currentUser.Id;

            if (!isCompanyOwner && !isShopUser)
                throw new UnauthorizedAccessException("Not authorized to edit this shop.");
            if (shop == null)
                throw new KeyNotFoundException("Shop not found.");
             
            // Update allowed fields
            shop.Name = shopDto.Name;
            shop.Phone = shopDto.Phone;
            shop.Address = shopDto.Address;
            await _shopRepo.UpdateAsync(shop);
        }


        // 4. Delete shop (only company owner or admin)
        public async Task DeleteShopAsync(int shopId, IdentityUser currentUser)
        {
            var shop = await _shopRepo.GetByIdAsync(shopId);
            if (shop == null)
                throw new KeyNotFoundException("Shop not found.");

            var company = await _companyRepo.GetByIdAsync(shop.CompanyId);

            bool isCompanyOwner = company != null && company.UserId == currentUser.Id;
            bool isShopUser = shop.UserId == currentUser.Id;

            if (!isCompanyOwner && !isShopUser)
                throw new UnauthorizedAccessException("Not authorized to delete this shop.");

            await _shopRepo.DeleteAsync(shop);

            // Optionally: Remove the IdentityUser for the shop
            var identityUser = await _userManager.FindByIdAsync(shop.UserId);
            if (identityUser != null)
                await _userManager.DeleteAsync(identityUser);
        }


        // 5. Get shop by ID (only company owner or admin)
        public async Task<ShopDto?> GetShopByIdAsync(int shopId, IdentityUser currentUser)
        {
            var shop = await _shopRepo.GetByIdAsync(shopId);
            if (shop == null)
                return null;

            var company = await _companyRepo.GetByIdAsync(shop.CompanyId);
            if (company == null || (!await IsAdminAsync(currentUser) && company.UserId != currentUser.Id))
                throw new UnauthorizedAccessException("Not authorized to view this shop.");

            return _mapper.Map<ShopDto>(shop);
        }

        // --- Authorization Helper ---
        private async Task<bool> IsAdminAsync(IdentityUser user)
        {
            // Use UserManager to check roles
            return user != null && await _userManager.IsInRoleAsync(user, "admin");
        }
    }
}
