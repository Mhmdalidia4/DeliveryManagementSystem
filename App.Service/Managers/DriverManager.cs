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
        private readonly IMapper _mapper;
        private readonly UserManager<IdentityUser> _userManager;

        public DriverManager(
            IBaseRepository<Driver> driverRepo,
            IBaseRepository<Company> companyRepo,
            UserManager<IdentityUser> userManager,
            IMapper mapper)
        {
            _driverRepo = driverRepo;
            _companyRepo = companyRepo;
            _userManager = userManager;
            _mapper = mapper;
        }

        // 1. Get all drivers for a company (only company owner)
        public async Task<IEnumerable<DriverDto>> GetAllDriversByCompanyAsync(int companyId, IdentityUser currentUser)
        {
            var company = await _companyRepo.GetByIdAsync(companyId);
            if (company == null || company.UserId != currentUser.Id)
                throw new UnauthorizedAccessException("Not authorized to view drivers of this company.");

            var drivers = await _driverRepo.FindAsync(d => d.CompanyId == companyId);
            return _mapper.Map<IEnumerable<DriverDto>>(drivers);
        }

        // 2. Add driver (create IdentityUser, then Driver; only company owner can add)
        public async Task<DriverDto> AddDriverAsync(
            DriverDto driverDto,
            string driverEmail,
            string driverPassword,
            IdentityUser currentUser)
        {
            // Ensure company exists and current user is owner
            var company = await _companyRepo.GetByIdAsync(driverDto.CompanyId);
            if (company == null || company.UserId != currentUser.Id)
                throw new UnauthorizedAccessException("Not authorized to add drivers to this company.");

            // 1. Create IdentityUser for driver
            var identityUser = new IdentityUser
            {
                UserName = driverEmail,
                Email = driverEmail,
            };
            var result = await _userManager.CreateAsync(identityUser, driverPassword);
            if (!result.Succeeded)
                throw new Exception($"Failed to create driver user: {string.Join(", ", result.Errors.Select(e => e.Description))}");

            // Optionally add driver to 'driver' role
            await _userManager.AddToRoleAsync(identityUser, "driver");

            // 2. Map and add Driver
            var driver = _mapper.Map<Driver>(driverDto);
            driver.UserId = identityUser.Id; // Reference to new IdentityUser
            await _driverRepo.AddAsync(driver);

            // Return the created DriverDto (with new DriverId)
            return _mapper.Map<DriverDto>(driver);
        }

        // 3. Edit driver (only driver themselves or company owner)
        public async Task EditDriverAsync(DriverDto driverDto, IdentityUser currentUser)
        {
            var driver = await _driverRepo.GetByIdAsync(driverDto.DriverId);
            if (driver == null)
                throw new KeyNotFoundException("Driver not found.");

            var company = await _companyRepo.GetByIdAsync(driver.CompanyId);

            // Only driver or company owner can edit
            if (
                (driver.UserId != currentUser.Id) &&
                (company == null || company.UserId != currentUser.Id)
            )
                throw new UnauthorizedAccessException("Not authorized to edit this driver.");

            // Update allowed fields only
            driver.Name = driverDto.Name;
            driver.Phone = driverDto.Phone;
            driver.IsAvailable = driverDto.IsAvailable;

            await _driverRepo.UpdateAsync(driver);
        }

        // 4. Delete driver (only company owner)
        public async Task DeleteDriverAsync(int driverId, IdentityUser currentUser)
        {
            var driver = await _driverRepo.GetByIdAsync(driverId);
            if (driver == null)
                throw new KeyNotFoundException("Driver not found.");

            var company = await _companyRepo.GetByIdAsync(driver.CompanyId);
            if (company == null || company.UserId != currentUser.Id)
                throw new UnauthorizedAccessException("Not authorized to delete this driver.");

            await _driverRepo.DeleteAsync(driver);

            // Optionally: You may also want to remove the IdentityUser for the driver!
             var identityUser = await _userManager.FindByIdAsync(driver.UserId);
             if (identityUser != null)
                 await _userManager.DeleteAsync(identityUser);
        }

        // 5. Get driver by ID (only driver themselves or company owner)
        public async Task<DriverDto?> GetDriverByIdAsync(int driverId, IdentityUser currentUser)
        {
            var driver = await _driverRepo.GetByIdAsync(driverId);
            if (driver == null)
                return null;

            var company = await _companyRepo.GetByIdAsync(driver.CompanyId);

            // Only driver or company owner can access
            if (
                (driver.UserId != currentUser.Id) &&
                (company == null || company.UserId != currentUser.Id)
            )
                throw new UnauthorizedAccessException("Not authorized to view this driver.");

            return _mapper.Map<DriverDto>(driver);
        }
    }
}
