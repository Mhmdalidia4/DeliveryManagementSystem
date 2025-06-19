using App.Domain.DTOs;
using App.Domain.Interfaces;
using App.Domain.Interfaces.Base;
using App.Domain.Models;
using App.Service.Interface;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using System.Diagnostics;

namespace App.Service.Managers
{
    public class PickUpRequestManager: IPickUpRequestManager
    {
        private readonly IBaseRepository<PickUpRequest> _pickUpRequestRepo;
        private readonly IBaseRepository<Shop> _shopRepo;
        private readonly IBaseRepository<Company> _companyRepo;
        private readonly IBaseRepository<Driver> _driverRepo;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IMapper _mapper;

        public PickUpRequestManager(
            IBaseRepository<PickUpRequest> pickUpRequestRepo,
            IBaseRepository<Shop> shopRepo,
            IBaseRepository<Company> companyRepo,
            IBaseRepository<Driver> driverRepo,
            UserManager<IdentityUser> userManager,
            IMapper mapper)
        {
            _pickUpRequestRepo = pickUpRequestRepo;
            _shopRepo = shopRepo;
            _companyRepo = companyRepo;
            _driverRepo = driverRepo;
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<IEnumerable<PickupRequestDto>> GetAllByUserAsync(IdentityUser currentUser)
        {
            var isCompany = await _userManager.IsInRoleAsync(currentUser, "company");
            var isShop = await _userManager.IsInRoleAsync(currentUser, "shop");
            var isDriver = await _userManager.IsInRoleAsync(currentUser, "driver");

            IEnumerable<PickUpRequest> pickups;

            if (isCompany)
            {
                var company = (await _companyRepo.FindAsync(c => c.UserId == currentUser.Id)).FirstOrDefault();
                if (company == null) throw new KeyNotFoundException("Company not found");
                pickups = await _pickUpRequestRepo.FindAsync(p => p.CompanyId == company.CompanyId);
            }
            else if (isShop)
            {
                var shop = (await _shopRepo.FindAsync(s => s.UserId == currentUser.Id)).FirstOrDefault();
                if (shop == null) throw new KeyNotFoundException("Shop not found");
                pickups = await _pickUpRequestRepo.FindAsync(p => p.ShopId == shop.ShopId);
            }
            else if (isDriver)
            {
                var driver = (await _driverRepo.FindAsync(d => d.UserId == currentUser.Id)).FirstOrDefault();
                if (driver == null) throw new KeyNotFoundException("Driver not found");
                pickups = await _pickUpRequestRepo.FindAsync(p => p.AssignedDriverId == driver.DriverId);
            }
            else
            {
                throw new UnauthorizedAccessException();
            }

            return _mapper.Map<IEnumerable<PickupRequestDto>>(pickups);
        }

        public async Task<PickupRequestDto> AddAsync(PickupRequestDto dto, IdentityUser currentUser)
        {
            var isShop = await _userManager.IsInRoleAsync(currentUser, "shop");
            var isCompany = await _userManager.IsInRoleAsync(currentUser, "company");

            if (!isShop && !isCompany)
                throw new UnauthorizedAccessException("Only shop or company users can create pickup requests.");

            Shop? targetShop = null;
            int companyId;

            if (isShop)
            {
                targetShop = (await _shopRepo.FindAsync(s => s.UserId == currentUser.Id)).FirstOrDefault();
                if (targetShop == null || targetShop.ShopId != dto.ShopId)
                    throw new UnauthorizedAccessException("Shop user can only create pickup requests for their own shop.");
                companyId = targetShop.CompanyId;
            }
            else // isCompany
            {
                var company = (await _companyRepo.FindAsync(c => c.UserId == currentUser.Id)).FirstOrDefault();
                if (company == null)
                    throw new KeyNotFoundException("Company not found.");

                targetShop = await _shopRepo.GetByIdAsync(dto.ShopId);
                if (targetShop == null || targetShop.CompanyId != company.CompanyId)
                    throw new UnauthorizedAccessException("Company can only add requests for its own shops.");

                companyId = company.CompanyId;
            }

            var entity = _mapper.Map<PickUpRequest>(dto);
            entity.CompanyId = companyId;
            entity.RequestedAt = DateTime.UtcNow;
            entity.IsCompleted = false;

            await _pickUpRequestRepo.AddAsync(entity);
            return _mapper.Map<PickupRequestDto>(entity);
        }


        public async Task DeleteAsync(int requestId, IdentityUser currentUser)
        {
            var entity = await _pickUpRequestRepo.GetByIdAsync(requestId);
            if (entity == null) throw new KeyNotFoundException();

            var isCompany = await _userManager.IsInRoleAsync(currentUser, "company");
            var isShop = await _userManager.IsInRoleAsync(currentUser, "shop");

            if (isCompany)
            {
                var company = await _companyRepo.GetByIdAsync(entity.CompanyId);
                if (company?.UserId != currentUser.Id)
                    throw new UnauthorizedAccessException();
            }
            else if (isShop)
            {
                if (entity.AssignedDriverId != null)
                    throw new InvalidOperationException("Cannot delete assigned request.");

                var shop = await _shopRepo.GetByIdAsync(entity.ShopId);
                if (shop?.UserId != currentUser.Id)
                    throw new UnauthorizedAccessException();
            }
            else
            {
                throw new UnauthorizedAccessException();
            }

            await _pickUpRequestRepo.DeleteAsync(entity);
        }

        public async Task AssignDriverAsync(int requestId, int driverId, IdentityUser currentUser)
        {
            var entity = await _pickUpRequestRepo.GetByIdAsync(requestId);
            if (entity == null) throw new KeyNotFoundException();

            var company = await _companyRepo.GetByIdAsync(entity.CompanyId);
            if (company?.UserId != currentUser.Id)
                throw new UnauthorizedAccessException();

            var driver = await _driverRepo.GetByIdAsync(driverId);
            if (driver == null || driver.CompanyId != company.CompanyId)
                throw new InvalidOperationException("Invalid driver assignment");

            entity.AssignedDriverId = driverId;
            await _pickUpRequestRepo.UpdateAsync(entity);
        }

        public async Task UnassignDriverAsync(int requestId, IdentityUser currentUser)
        {
            var entity = await _pickUpRequestRepo.GetByIdAsync(requestId);
            if (entity == null) throw new KeyNotFoundException();

            var company = await _companyRepo.GetByIdAsync(entity.CompanyId);
            if (company?.UserId != currentUser.Id)
                throw new UnauthorizedAccessException();

            entity.AssignedDriverId = null;
            await _pickUpRequestRepo.UpdateAsync(entity);
        }        public async Task UpdateAsync(PickupRequestDto dto, IdentityUser currentUser)
        {            var entity = await _pickUpRequestRepo.GetByIdAsync(dto.PickupRequestId);
            if (entity == null) throw new KeyNotFoundException("Pickup request not found");

            // Check user roles
            var isCompany = await _userManager.IsInRoleAsync(currentUser, "company");
            var isDriver = await _userManager.IsInRoleAsync(currentUser, "driver");
            
            if (!isCompany && !isDriver)
                throw new UnauthorizedAccessException("Only company users and drivers can update pickup requests.");

            if (isCompany)
            {
                // Company validation - can update its own pickup requests
                var company = (await _companyRepo.FindAsync(c => c.UserId == currentUser.Id)).FirstOrDefault();
                if (company == null || entity.CompanyId != company.CompanyId)
                    throw new UnauthorizedAccessException("Company can only update its own pickup requests.");

                // Validate the driver (if provided) belongs to the company
                if (dto.AssignedDriverId.HasValue)
                {
                    var driver = await _driverRepo.GetByIdAsync(dto.AssignedDriverId.Value);
                    if (driver == null || driver.CompanyId != company.CompanyId)
                        throw new UnauthorizedAccessException("Company can only assign its own drivers.");
                }
                
                // Company can update driver assignment and completion status
                entity.AssignedDriverId = dto.AssignedDriverId; // Can be null to unassign
                entity.IsCompleted = dto.IsCompleted; // Update completion status
            }            else if (isDriver)
            {
                // Driver validation - can only update pickup requests assigned to them
                var drivers = await _driverRepo.FindAsync(d => d.UserId == currentUser.Id);
                var driverRecord = drivers.FirstOrDefault();
                
                if (driverRecord == null)
                    throw new UnauthorizedAccessException("Driver record not found.");
                    
                if (entity.AssignedDriverId != driverRecord.DriverId)
                    throw new UnauthorizedAccessException("Driver can only update pickup requests assigned to them.");
                
                // Drivers can only change status from pending to completed, not back to pending
                if (entity.IsCompleted == true && dto.IsCompleted == false)
                    throw new UnauthorizedAccessException("You cannot change the status back to pending once it's completed.");
                  // Driver can only update completion status, not driver assignment
                entity.IsCompleted = dto.IsCompleted; // Only update completion status
                // Keep existing driver assignment - don't change it
            }
            
            await _pickUpRequestRepo.UpdateAsync(entity);
        }
    }
}