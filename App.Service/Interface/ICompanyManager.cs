using App.Domain.DTOs;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Service.Interface
{
    public interface ICompanyManager
    {
        Task<int> GetCompanyIdAsync(IdentityUser user);
        Task<IEnumerable<CompanyDto>> GetAllCompaniesAsync(IdentityUser currentUser);
        Task<CompanyDto?> GetCompanyByIdAsync(int companyId, IdentityUser currentUser);
        Task EditCompanyAsync(CompanyDto companyDto, IdentityUser currentUser);
        Task DeleteCompanyAsync(int companyId, IdentityUser currentUser);
        Task<CompanyDto?> AddLicenseToCompanyAsync(int companyId, int monthsToAdd, IdentityUser currentUser);
        Task<CompanyDto?> RemoveLicenseFromCompanyAsync(int companyId, IdentityUser currentUser);
        Task<CompanyDto> AddCompanyAsync(CompanyDto companyDto, string ownerEmail, string ownerPassword, IdentityUser currentUser);
    }
}
