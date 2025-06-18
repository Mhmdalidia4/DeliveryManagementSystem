using App.Domain.DTOs;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using App.Service.Interface;
namespace App.Service.Interface
{
    public interface IDriverManager
    {
        Task<IEnumerable<DriverDto>> GetAllDriversByCompanyAsync(int companyId, IdentityUser currentUser);
        Task<DriverDto> AddDriverAsync(DriverDto driverDto, string driverEmail, string driverPassword, IdentityUser currentUser);
        Task EditDriverAsync(DriverDto driverDto, IdentityUser currentUser);
        Task DeleteDriverAsync(int driverId, IdentityUser currentUser);
        Task<DriverDto?> GetDriverByIdAsync(int driverId, IdentityUser currentUser);
    }
}
