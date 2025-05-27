using AutoMapper;
using App.Domain.Models;
using App.Domain.DTOs;
using App.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using App.Domain.Interfaces.Base;

namespace App.Service.Managers
{
    public class DriverManager
    {
        private readonly IBaseRepository<Driver> _driverRepo;
        private readonly IBaseRepository<Company> _companyRepo;
        private readonly IBaseRepository<IdentityUser> _userRepo;
        private readonly IMapper _mapper;

        public DriverManager(
            IBaseRepository<Driver> driverRepo,
            IBaseRepository<Company> companyRepo,
            IBaseRepository<IdentityUser> userRepo,
            IMapper mapper)
        {
            _driverRepo = driverRepo;
            _companyRepo = companyRepo;
            _userRepo = userRepo;
            _mapper = mapper;
        }

        // 1. Get all drivers for a company (Admins, company owners)
        public async Task<IEnumerable<DriverDto>> GetAllDriversByCompanyAsync(int companyId, IdentityUser currentUser)
        {
            if (!await CanManageCompanyAsync(currentUser, companyId))
                throw new UnauthorizedAccessException("Not authorized to view drivers of this company.");

            var drivers = await _driverRepo.FindAsync(d => d.CompanyId == companyId);
            return _mapper.Map<IEnumerable<DriverDto>>(drivers);
        }

        // 2. Add driver (Admins, company owners)
        public async Task AddDriverAsync(DriverDto driverDto, IdentityUser currentUser)
        {
            if (!await CanManageCompanyAsync(currentUser, driverDto.CompanyId))
                throw new UnauthorizedAccessException("Not authorized to add drivers to this company.");

            var driver = _mapper.Map<Driver>(driverDto);
            await _driverRepo.AddAsync(driver);
        }

        // 3. Edit driver (Admins, company owners, the driver themself)
        public async Task EditDriverAsync(DriverDto driverDto, IdentityUser currentUser)
        {
            var driver = await _driverRepo.GetByIdAsync(driverDto.DriverId);
            if (driver == null)
                throw new KeyNotFoundException("Driver not found.");

            if (
                !await CanManageCompanyAsync(currentUser, driver.CompanyId) &&
                !IsDriverUser(currentUser, driver)
            )
                throw new UnauthorizedAccessException("Not authorized to edit this driver.");

            // Update allowed fields only
            driver.Name = driverDto.Name;
            driver.Phone = driverDto.Phone;
            driver.IsAvailable = driverDto.IsAvailable;

            await _driverRepo.UpdateAsync(driver);
        }

        // 4. Delete driver (Admins, company owners)
        public async Task DeleteDriverAsync(int driverId, IdentityUser currentUser)
        {
            var driver = await _driverRepo.GetByIdAsync(driverId);
            if (driver == null)
                throw new KeyNotFoundException("Driver not found.");

            if (!await CanManageCompanyAsync(currentUser, driver.CompanyId))
                throw new UnauthorizedAccessException("Not authorized to delete this driver.");

            await _driverRepo.DeleteAsync(driver);
        }

        // 5. Get driver by ID (Admins, company owner, or the driver themselves)
        public async Task<DriverDto?> GetDriverByIdAsync(int driverId, IdentityUser currentUser)
        {
            var driver = await _driverRepo.GetByIdAsync(driverId);
            if (driver == null)
                return null;

            if (
                !await CanManageCompanyAsync(currentUser, driver.CompanyId) &&
                !IsDriverUser(currentUser, driver)
            )
                throw new UnauthorizedAccessException("Not authorized to view this driver.");

            return _mapper.Map<DriverDto>(driver);
        }

        // --- Authorization Helpers ---

        private async Task<bool> CanManageCompanyAsync(IdentityUser user, int companyId)
        {
            if (await IsAdminAsync(user))
                return true;

            var company = await _companyRepo.GetByIdAsync(companyId);
            return company != null && user != null && company.UserId == user.Id;
        }

        private async Task<bool> IsAdminAsync(IdentityUser user)
        {
            // Replace with your real role/claims check!
            return user != null && user.UserName == "admin";
        }

        private bool IsDriverUser(IdentityUser user, Driver driver)
        {
            return user != null && driver.UserId == user.Id;
        }
    }
}
