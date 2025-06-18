using App.Domain.DTOs;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Service.Interface
{
    public interface ISettingsManager
    {
        // Order Areas
        Task<List<OrderAreaDto>> GetAllOrderAreasAsync(IdentityUser currentUser);
        Task<OrderAreaDto> AddOrderAreaAsync(OrderAreaDto dto, IdentityUser currentUser);
        Task DeleteOrderAreaAsync(int id, IdentityUser currentUser);
        Task<OrderAreaDto> UpdateOrderAreaAsync(OrderAreaDto dto, IdentityUser currentUser);

        // Order Statuses
        Task<List<OrderStatusDto>> GetAllOrderStatusesAsync(IdentityUser currentUser);
        Task<OrderStatusDto> AddOrderStatusAsync(OrderStatusDto dto, IdentityUser currentUser);
        Task DeleteOrderStatusAsync(int id, IdentityUser currentUser);

        // Payment Methods
        Task<List<PaymentMethodDto>> GetAllPaymentMethodsAsync(IdentityUser currentUser);
        Task<PaymentMethodDto> AddPaymentMethodAsync(PaymentMethodDto dto, IdentityUser currentUser);
        Task DeletePaymentMethodAsync(int id, IdentityUser currentUser);
    }
}
