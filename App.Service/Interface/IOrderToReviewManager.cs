using App.Domain.DTOs;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Service.Interface
{
    public interface IOrderToReviewManager
    {
        Task<IEnumerable<OrdersToReviewDto>> GetAllOrdersToReviewByUserAsync(IdentityUser currentUser);
        Task<OrdersToReviewDto> AddOrderToReviewAsync(OrdersToReviewDto dto, IdentityUser currentUser);
        Task EditOrderToReviewAsync(OrdersToReviewDto dto, IdentityUser currentUser);
        Task DeleteOrderToReviewAsync(int orderToReviewId, IdentityUser currentUser);
        Task<IEnumerable<OrdersToReviewDto>> FilterOrdersToReviewAsync(IdentityUser currentUser, int? orderToReviewId = null, int? shopId = null, int? orderAreaId = null, string? customerName = null);
        Task AcceptOrderAsync(int orderToReviewId, IdentityUser currentUser);
        Task RejectOrderAsync(int orderToReviewId, IdentityUser currentUser);
    }
}
