using App.Domain.DTOs;
using App.Domain.Interfaces.Base;
using App.Domain.Models;
using App.Service.Interface;
using AutoMapper;
using Microsoft.AspNetCore.Identity;

namespace App.Services.Managers
{
    public class SettingsManager: ISettingsManager
    {
        private readonly IBaseRepository<OrderArea> _orderAreaRepo;
        private readonly IBaseRepository<OrderStatus> _orderStatusRepo;
        private readonly IBaseRepository<PaymentMethod> _paymentMethodRepo;
        private readonly IBaseRepository<Company> _companyRepo;
        private readonly IMapper _mapper;
        private readonly UserManager<IdentityUser> _userManager;

        public SettingsManager(
            IBaseRepository<OrderArea> orderAreaRepo,
            IBaseRepository<OrderStatus> orderStatusRepo,
            IBaseRepository<PaymentMethod> paymentMethodRepo,
            IBaseRepository<Company> companyRepo,
            IMapper mapper,
            UserManager<IdentityUser> userManager)
        {
            _orderAreaRepo = orderAreaRepo;
            _orderStatusRepo = orderStatusRepo;
            _paymentMethodRepo = paymentMethodRepo;
            _companyRepo = companyRepo;
            _mapper = mapper;
            _userManager = userManager;
        }

        private async Task<int> GetCompanyIdAsync(IdentityUser currentUser)
        {
            // Ensure the user is in the "Company" role
            var isCompany = await _userManager.IsInRoleAsync(currentUser, "Company");
            if (!isCompany)
                throw new UnauthorizedAccessException("Access denied. Only company users can access settings.");

            // Get the CompanyId tied to the logged-in user
            var company = (await _companyRepo.FindAsync(c => c.UserId == currentUser.Id)).FirstOrDefault();
            if (company == null)
                throw new UnauthorizedAccessException("Company not found for the current user.");

            return company.CompanyId;
        }


        // -------------------- ORDER AREA --------------------

        public async Task<List<OrderAreaDto>> GetAllOrderAreasAsync(IdentityUser currentUser)
        {
            int companyId = await GetCompanyIdAsync(currentUser);
            var entities = await _orderAreaRepo.FindAsync(o => o.CompanyId == companyId);
            return _mapper.Map<List<OrderAreaDto>>(entities);
        }

        public async Task<OrderAreaDto> AddOrderAreaAsync(OrderAreaDto dto, IdentityUser currentUser)
        {
            int companyId = await GetCompanyIdAsync(currentUser);
            var entity = _mapper.Map<OrderArea>(dto);
            entity.CompanyId = companyId;
            await _orderAreaRepo.AddAsync(entity);
            return _mapper.Map<OrderAreaDto>(entity);
        }

        public async Task DeleteOrderAreaAsync(int id, IdentityUser currentUser)
        {
            int companyId = await GetCompanyIdAsync(currentUser);
            var entity = await _orderAreaRepo.GetByIdAsync(id);
            if (entity == null || entity.CompanyId != companyId)
                throw new UnauthorizedAccessException("You can only delete your own order areas.");

            await _orderAreaRepo.DeleteAsync(entity);
        }

        public async Task<OrderAreaDto> UpdateOrderAreaAsync(OrderAreaDto dto, IdentityUser currentUser)
        {
            int companyId = await GetCompanyIdAsync(currentUser);
            var entity = await _orderAreaRepo.GetByIdAsync(dto.OrderAreaId);
            if (entity == null || entity.CompanyId != companyId)
                throw new UnauthorizedAccessException("You can only update your own order areas.");

            entity.Name = dto.Name;
            entity.DeliveryFees = dto.DeliveryFees;

            await _orderAreaRepo.UpdateAsync(entity);
            return _mapper.Map<OrderAreaDto>(entity);
        }

        // -------------------- ORDER STATUS --------------------

        public async Task<List<OrderStatusDto>> GetAllOrderStatusesAsync(IdentityUser currentUser)
        {
            int companyId = await GetCompanyIdAsync(currentUser);
            var entities = await _orderStatusRepo.FindAsync(s => s.CompanyId == companyId);
            return _mapper.Map<List<OrderStatusDto>>(entities);
        }

        public async Task<OrderStatusDto> AddOrderStatusAsync(OrderStatusDto dto, IdentityUser currentUser)
        {
            int companyId = await GetCompanyIdAsync(currentUser);
            var entity = _mapper.Map<OrderStatus>(dto);
            entity.CompanyId = companyId;
            await _orderStatusRepo.AddAsync(entity);
            return _mapper.Map<OrderStatusDto>(entity);
        }

        public async Task DeleteOrderStatusAsync(int id, IdentityUser currentUser)
        {
            int companyId = await GetCompanyIdAsync(currentUser);
            var entity = await _orderStatusRepo.GetByIdAsync(id);
            if (entity == null || entity.CompanyId != companyId)
                throw new UnauthorizedAccessException("You can only delete your own order statuses.");

            await _orderStatusRepo.DeleteAsync(entity);
        }

        // -------------------- PAYMENT METHOD --------------------

        public async Task<List<PaymentMethodDto>> GetAllPaymentMethodsAsync(IdentityUser currentUser)
        {
            int companyId = await GetCompanyIdAsync(currentUser);
            var entities = await _paymentMethodRepo.FindAsync(p => p.CompanyId == companyId);
            return _mapper.Map<List<PaymentMethodDto>>(entities);
        }

        public async Task<PaymentMethodDto> AddPaymentMethodAsync(PaymentMethodDto dto, IdentityUser currentUser)
        {
            int companyId = await GetCompanyIdAsync(currentUser);
            var entity = _mapper.Map<PaymentMethod>(dto);
            entity.CompanyId = companyId;
            await _paymentMethodRepo.AddAsync(entity);
            return _mapper.Map<PaymentMethodDto>(entity);
        }

        public async Task DeletePaymentMethodAsync(int id, IdentityUser currentUser)
        {
            int companyId = await GetCompanyIdAsync(currentUser);
            var entity = await _paymentMethodRepo.GetByIdAsync(id);
            if (entity == null || entity.CompanyId != companyId)
                throw new UnauthorizedAccessException("You can only delete your own payment methods.");

            await _paymentMethodRepo.DeleteAsync(entity);
        }
    }
}
