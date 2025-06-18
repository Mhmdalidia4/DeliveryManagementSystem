using App.Domain.DTOs;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Service.Interface
{
    public interface IShopManager
    {
        Task<IEnumerable<ShopDto>> GetAllShopsForCurrentCompanyUserAsync(IdentityUser currentUser);
        Task<ShopDto> AddShopAsync(ShopDto shopDto, string shopEmail, string shopPassword, IdentityUser currentUser);
        Task EditShopAsync(ShopDto shopDto, IdentityUser currentUser);
        Task DeleteShopAsync(int shopId, IdentityUser currentUser);
        Task<ShopDto?> GetShopByIdAsync(int shopId, IdentityUser currentUser);
    }
}
