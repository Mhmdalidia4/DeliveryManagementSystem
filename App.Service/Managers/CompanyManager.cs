using App.Domain.DTOs;
using App.Domain.Interfaces;
using App.Domain.Models;
using AutoMapper;
using global::App.Domain.DTOs;
using global::AutoMapper;
using Microsoft.AspNetCore.Identity;
using App.Domain.Models;
using App.Domain.Interfaces.Base;

namespace App.Service.Managers
{
    public class CompanyManager{
        private readonly IBaseRepository<Company> _companyRepo;
        private readonly IBaseRepository<IdentityUser> _userRepo;
        private readonly IMapper _mapper;

        public CompanyManager(
            IBaseRepository<Company> companyRepo,
            IBaseRepository<IdentityUser> userRepo,
            IMapper mapper)
        {
            _companyRepo = companyRepo;
            _userRepo = userRepo;
            _mapper = mapper;
        }

        // 1. Get all companies (Only allowed for admins)
        public async Task<IEnumerable<CompanyDto>> GetAllCompaniesAsync(IdentityUser currentUser)
        {
            if (!await IsAdminAsync(currentUser))
                throw new UnauthorizedAccessException("Not authorized to view all companies.");

            var companies = await _companyRepo.FindAsync(_ => true);
            return _mapper.Map<IEnumerable<CompanyDto>>(companies);
        }


        // 3. Edit company (Only admin or the company user themself)
        public async Task EditCompanyAsync(CompanyDto companyDto, IdentityUser currentUser)
        {
            var company = await _companyRepo.GetByIdAsync(companyDto.CompanyId);
            if (company == null)
                throw new KeyNotFoundException("Company not found.");

            if (!await CanAccessCompanyAsync(currentUser, company))
                throw new UnauthorizedAccessException("Not authorized to edit this company.");

            // Update only the allowed fields
            company.Name = companyDto.Name;
            company.Phone = companyDto.Phone;
            company.Address = companyDto.Address;
            await _companyRepo.UpdateAsync(company);
        }

        // 4. Delete company (Admins only)
        public async Task DeleteCompanyAsync(int companyId, IdentityUser currentUser)
        {
            if (!await IsAdminAsync(currentUser))
                throw new UnauthorizedAccessException("Not authorized to delete company.");

            var company = await _companyRepo.GetByIdAsync(companyId);
            if (company != null)
                await _companyRepo.DeleteAsync(company);
        }

        // 5. Get company by id (Admins or owner)
        public async Task<CompanyDto?> GetCompanyByIdAsync(int companyId, IdentityUser currentUser)
        {
            var company = await _companyRepo.GetByIdAsync(companyId);
            if (company == null)
                return null;

            if (!await CanAccessCompanyAsync(currentUser, company))
                throw new UnauthorizedAccessException("Not authorized to view this company.");

            return _mapper.Map<CompanyDto>(company);
        }

        // --- Authorization Helpers ---

        private async Task<bool> IsAdminAsync(IdentityUser user)
        {
            // Example: Check if the user is in the "Admin" role
            // If you use UserManager<IdentityUser>, replace this with real check
            // This is a stub for demonstration:
            return user != null && user.UserName == "admin";
            // TODO: Replace with your real role/claim checks!
        }

        private async Task<bool> CanAccessCompanyAsync(IdentityUser user, Company company)
        {
            if (await IsAdminAsync(user))
                return true;

            // Owner logic: company.UserId == user.Id
            return user != null && company.UserId == user.Id;
        }
        public async Task<CompanyDto?> AddLicenseToCompanyAsync(int companyId, int monthsToAdd, IdentityUser currentUser)
        {
            if (!await IsAdminAsync(currentUser))
                throw new UnauthorizedAccessException("Not authorized to add license.");

            var company = await _companyRepo.GetByIdAsync(companyId);
            if (company == null)
                throw new KeyNotFoundException("Company not found.");

            // Only update the LicenseEndDate
            if (company.LicenseEndDate.HasValue && company.LicenseEndDate.Value > DateOnly.FromDateTime(DateTime.Now))
                company.LicenseEndDate = company.LicenseEndDate.Value.AddMonths(monthsToAdd); // extend from current end date
            else
                company.LicenseEndDate = DateOnly.FromDateTime(DateTime.Now.AddMonths(monthsToAdd)); // set new period from now

            await _companyRepo.UpdateAsync(company);
            return _mapper.Map<CompanyDto>(company);
        }

        public async Task<CompanyDto?> RemoveLicenseFromCompanyAsync(int companyId, IdentityUser currentUser)
        {
            if (!await IsAdminAsync(currentUser))
                throw new UnauthorizedAccessException("Not authorized to remove license.");

            var company = await _companyRepo.GetByIdAsync(companyId);
            if (company == null)
                throw new KeyNotFoundException("Company not found.");

            // Set LicenseEndDate to now, keep start date as is
            company.LicenseEndDate = DateOnly.FromDateTime(DateTime.Now);
            await _companyRepo.UpdateAsync(company);

            return _mapper.Map<CompanyDto>(company);
        }

    }
}

