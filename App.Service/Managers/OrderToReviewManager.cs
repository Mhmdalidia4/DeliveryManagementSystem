using App.Domain.DTOs;
using App.Domain.Interfaces;
using App.Domain.Interfaces.Base;
using App.Domain.Models;
using App.Service.Interface;
using AutoMapper;
using Microsoft.AspNetCore.Identity;

namespace App.Service.Managers
{
    public class OrderToReviewManager: IOrderToReviewManager
    {
        private readonly IBaseRepository<OrdersToReview> _orderToReviewRepo;
        private readonly IBaseRepository<Shop> _shopRepo;
        private readonly IBaseRepository<Company> _companyRepo;
        private readonly IBaseRepository<Order> _orderRepo;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IMapper _mapper;

        public OrderToReviewManager(
            IBaseRepository<OrdersToReview> orderToReviewRepo,
            IBaseRepository<Shop> shopRepo,
            IBaseRepository<Company> companyRepo,
            IBaseRepository<Order> orderRepo,
            UserManager<IdentityUser> userManager,
            IMapper mapper)
        {
            _orderToReviewRepo = orderToReviewRepo;
            _shopRepo = shopRepo;
            _companyRepo = companyRepo;
            _orderRepo = orderRepo;
            _userManager = userManager;
            _mapper = mapper;
        }

        // 1. Get all OrdersToReview for a user (admin, shop, or company owner)
        public async Task<IEnumerable<OrdersToReviewDto>> GetAllOrdersToReviewByUserAsync(IdentityUser currentUser)
        {
            var isShop = await _userManager.IsInRoleAsync(currentUser, "shop");
            var isCompany = await _userManager.IsInRoleAsync(currentUser, "company");

            if (!( isShop || isCompany))
                throw new UnauthorizedAccessException("Not authorized.");

            IEnumerable<OrdersToReview> orders;

           
            if (isCompany)
            {
                var company = (await _companyRepo.FindAsync(c => c.UserId == currentUser.Id)).FirstOrDefault();
                if (company == null)
                    throw new KeyNotFoundException("No company found for this user.");
                orders = await _orderToReviewRepo.FindAsync(o => o.CompanyId == company.CompanyId);
            }
            else // shop
            {
                var shop = (await _shopRepo.FindAsync(s => s.UserId == currentUser.Id)).FirstOrDefault();
                if (shop == null)
                    throw new KeyNotFoundException("No shop found for this user.");
                orders = await _orderToReviewRepo.FindAsync(o => o.ShopId == shop.ShopId);
            }

            return _mapper.Map<IEnumerable<OrdersToReviewDto>>(orders.Where(o => o.Status != false));
        }

        // 2. Add OrderToReview (only shop)
        public async Task<OrdersToReviewDto> AddOrderToReviewAsync(OrdersToReviewDto dto, IdentityUser currentUser)
        {
            if (!await _userManager.IsInRoleAsync(currentUser, "shop"))
                throw new UnauthorizedAccessException("Only shop users can add orders.");

            var shop = (await _shopRepo.FindAsync(s => s.UserId == currentUser.Id)).FirstOrDefault();
            if (shop == null || shop.ShopId != dto.ShopId)
                throw new UnauthorizedAccessException("You can only add orders to your own shop.");

            var entity = _mapper.Map<OrdersToReview>(dto);
            entity.Status = true; // default: active (not rejected)
            await _orderToReviewRepo.AddAsync(entity);
            return _mapper.Map<OrdersToReviewDto>(entity);
        }

        // 3. Edit OrderToReview (only shop)
        public async Task EditOrderToReviewAsync(OrdersToReviewDto dto, IdentityUser currentUser)
        {
            if (!await _userManager.IsInRoleAsync(currentUser, "shop"))
                throw new UnauthorizedAccessException("Only shop users can edit orders.");

            var entity = await _orderToReviewRepo.GetByIdAsync(dto.OrderToReviewId);
            if (entity == null)
                throw new KeyNotFoundException("OrderToReview not found.");

            var shop = await _shopRepo.GetByIdAsync(entity.ShopId);
            if (shop == null || shop.UserId != currentUser.Id)
                throw new UnauthorizedAccessException("You can only edit your own shop's orders.");

            // Update allowed fields
            entity.CustomerName = dto.CustomerName;
            entity.CustomerPhone = dto.CustomerPhone;
            entity.CustomerAddress = dto.CustomerAddress;
            entity.OrderAreaId = dto.OrderAreaId;
            entity.Note = dto.Note;
            entity.Amount = dto.Amount;
            entity.TrackingPassword = dto.TrackingPassword;
            entity.Status = dto.Status;
            entity.CreatedAt = dto.CreatedAt;

            await _orderToReviewRepo.UpdateAsync(entity);
        }

        // 4. Delete OrderToReview (shop or company owner)
        public async Task DeleteOrderToReviewAsync(int orderToReviewId, IdentityUser currentUser)
        {
            var entity = await _orderToReviewRepo.GetByIdAsync(orderToReviewId);
            if (entity == null)
                throw new KeyNotFoundException("OrderToReview not found.");

            var shop = await _shopRepo.GetByIdAsync(entity.ShopId);
            var company = shop != null ? await _companyRepo.GetByIdAsync(shop.CompanyId) : null;

            bool isShopUser = shop != null && shop.UserId == currentUser.Id;
            bool isCompanyOwner = company != null && company.UserId == currentUser.Id;

            if (!isShopUser && !isCompanyOwner)
                throw new UnauthorizedAccessException("Not authorized to delete this order.");

            await _orderToReviewRepo.DeleteAsync(entity);
        }

        // 5. Filter by fields (admin, shop, company)
        public async Task<IEnumerable<OrdersToReviewDto>> FilterOrdersToReviewAsync(
            IdentityUser currentUser,
            int? orderToReviewId = null,
            int? shopId = null,
            int? orderAreaId = null,
            string? customerName = null)
        {
            var isAdmin = await _userManager.IsInRoleAsync(currentUser, "admin");
            var isShop = await _userManager.IsInRoleAsync(currentUser, "shop");
            var isCompany = await _userManager.IsInRoleAsync(currentUser, "company");

            if (!(isAdmin || isShop || isCompany))
                throw new UnauthorizedAccessException("Not authorized.");

            IEnumerable<OrdersToReview> orders;

            if (isAdmin)
            {
                orders = await _orderToReviewRepo.FindAsync(_ => true);
            }
            else if (isCompany)
            {
                var company = (await _companyRepo.FindAsync(c => c.UserId == currentUser.Id)).FirstOrDefault();
                if (company == null)
                    throw new KeyNotFoundException("No company found for this user.");
                orders = await _orderToReviewRepo.FindAsync(o => o.CompanyId == company.CompanyId);
            }
            else // shop
            {
                var shop = (await _shopRepo.FindAsync(s => s.UserId == currentUser.Id)).FirstOrDefault();
                if (shop == null)
                    throw new KeyNotFoundException("No shop found for this user.");
                orders = await _orderToReviewRepo.FindAsync(o => o.ShopId == shop.ShopId);
            }

            var filtered = orders.Where(o =>
                (orderToReviewId == null || o.OrderToReviewId == orderToReviewId) &&
                (shopId == null || o.ShopId == shopId) &&
                (orderAreaId == null || o.OrderAreaId == orderAreaId) &&
                (customerName == null || (o.CustomerName != null && o.CustomerName.Contains(customerName))) &&
                o.Status != false // Exclude rejected
            );

            return _mapper.Map<IEnumerable<OrdersToReviewDto>>(filtered);
        }

        // 6. Accept order (delete from OrdersToReview, add to Order)
        public async Task AcceptOrderAsync(int orderToReviewId, IdentityUser currentUser)
        {
            var entity = await _orderToReviewRepo.GetByIdAsync(orderToReviewId);
            if (entity == null)
                throw new KeyNotFoundException("OrderToReview not found.");

            var shop = await _shopRepo.GetByIdAsync(entity.ShopId);
            var company = shop != null ? await _companyRepo.GetByIdAsync(shop.CompanyId) : null;

            bool isShopUser = shop != null && shop.UserId == currentUser.Id;
            bool isCompanyOwner = company != null && company.UserId == currentUser.Id;
            if (!isShopUser && !isCompanyOwner)
                throw new UnauthorizedAccessException("Not authorized to accept this order.");

            // Create Order from OrdersToReview
            var order = new Order
            {
                CompanyId = entity.CompanyId,
                ShopId = entity.ShopId,
                CustomerName = entity.CustomerName,
                CustomerPhone = entity.CustomerPhone,
                CustomerAddress = entity.CustomerAddress,
                OrderAreaId = entity.OrderAreaId,
                Note = entity.Note,
                Amount = entity.Amount,
                TrackingPassword = entity.TrackingPassword,
                CreatedAt = DateTime.Now
            };

            await _orderRepo.AddAsync(order);
            await _orderToReviewRepo.DeleteAsync(entity);
        }

        // 7. Reject order (set Status to false)
        public async Task RejectOrderAsync(int orderToReviewId, IdentityUser currentUser)
        {
            var entity = await _orderToReviewRepo.GetByIdAsync(orderToReviewId);
            if (entity == null)
                throw new KeyNotFoundException("OrderToReview not found.");

            var shop = await _shopRepo.GetByIdAsync(entity.ShopId);
            var company = shop != null ? await _companyRepo.GetByIdAsync(shop.CompanyId) : null;

            bool isShopUser = shop != null && shop.UserId == currentUser.Id;
            bool isCompanyOwner = company != null && company.UserId == currentUser.Id;
            if (!isShopUser && !isCompanyOwner)
                throw new UnauthorizedAccessException("Not authorized to reject this order.");

            entity.Status = false; // Mark as rejected
            await _orderToReviewRepo.UpdateAsync(entity);
        }
    }
}
