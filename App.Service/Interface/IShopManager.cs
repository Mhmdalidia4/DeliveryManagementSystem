using App.Domain.DTOs;
using App.Domain.Models;
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
        Task<int> GetShopIdByUserAsync(IdentityUser user);
        Task<IEnumerable<ShopDto>> GetAllShopsForCompanyUserAsync(int companyid);
        Task<string?> GetShopNameByIdAsync(int shopId);
        Task<IEnumerable<ShopDto>> GetAllShopsForCurrentCompanyUserAsync(IdentityUser currentUser);
        Task<ShopDto> AddShopAsync(ShopDto shopDto, string shopEmail, string shopPassword, IdentityUser currentUser);
        Task EditShopAsync(ShopDto shopDto, IdentityUser currentUser);
        Task DeleteShopAsync(int shopId, IdentityUser currentUser);
        Task<ShopDto?> GetShopByIdAsync(int shopId, IdentityUser currentUser);
        Task<Shop> GetShopEntityByIdAsync(int shopId);
    }
}
