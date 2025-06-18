using App.Domain.DTOs;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Service.Interface
{
    public interface IOrderManager
    {
        Task<List<OrderDto>> FilterOrdersAsync(OrderDto filterDto, IdentityUser currentUser);
        Task<List<OrderDto>> GetOrdersForUserAsync(IdentityUser currentUser);
        Task<OrderDto> AddOrderAsync(OrderDto dto, IdentityUser currentUser);
        Task<OrderDto> UpdateOrderAsync(OrderDto dto, IdentityUser currentUser);
        Task DeleteOrderAsync(int orderId, IdentityUser currentUser);
    }
}
