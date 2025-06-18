using App.Domain.DTOs;
using App.Domain.Interfaces.Base;
using App.Domain.Models;
using App.Service.Interface;
using App.Service.Managers;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace App.Services.Managers
{
    public class OrderManager: IOrderManager
    {
        private readonly IBaseRepository<Order> _orderRepo;
        private readonly IMapper _mapper;
        private readonly CompanyManager _userCompanyService;

        public OrderManager(
            IBaseRepository<Order> orderRepo,
            IMapper mapper,
            CompanyManager userCompanyService)
        {
            _orderRepo = orderRepo;
            _mapper = mapper;
            _userCompanyService = userCompanyService;
        }

        private async Task<int> GetCompanyIdAsync(IdentityUser currentUser)
        {
            return await _userCompanyService.GetCompanyIdAsync(currentUser);
        }
       
        // 1. Dynamic filter method
        public async Task<List<OrderDto>> FilterOrdersAsync(OrderDto filterDto, IdentityUser currentUser)
        {
            int companyId = await GetCompanyIdAsync(currentUser);

            // Build dynamic expression
            Expression<Func<Order, bool>> predicate = o => o.CompanyId == companyId;

            if (filterDto.OrderId != 0)
                predicate = predicate.AndAlso(o => o.OrderId == filterDto.OrderId);

            if (!string.IsNullOrEmpty(filterDto.CustomerName))
                predicate = predicate.AndAlso(o => o.CustomerName != null && o.CustomerName.Contains(filterDto.CustomerName));

            if (!string.IsNullOrEmpty(filterDto.CustomerPhone))
                predicate = predicate.AndAlso(o => o.CustomerPhone != null && o.CustomerPhone.Contains(filterDto.CustomerPhone));

            if (filterDto.OrderAreaId.HasValue)
                predicate = predicate.AndAlso(o => o.OrderAreaId == filterDto.OrderAreaId);

            if (filterDto.OrderStatusId.HasValue)
                predicate = predicate.AndAlso(o => o.OrderStatusId == filterDto.OrderStatusId);

            if (filterDto.IsCollectedFromDriver.HasValue)
                predicate = predicate.AndAlso(o => o.IsCollectedFromDriver == filterDto.IsCollectedFromDriver);

            if (filterDto.IsCollectedFromCustomer.HasValue)
                predicate = predicate.AndAlso(o => o.IsCollectedFromCustomer == filterDto.IsCollectedFromCustomer);

            if (filterDto.IsPaidToShop.HasValue)
                predicate = predicate.AndAlso(o => o.IsPaidToShop == filterDto.IsPaidToShop);

            if (!string.IsNullOrEmpty(filterDto.TrackingPassword))
                predicate = predicate.AndAlso(o => o.TrackingPassword != null && o.TrackingPassword.Contains(filterDto.TrackingPassword));

            if (filterDto.ShopId.HasValue)
                predicate = predicate.AndAlso(o => o.ShopId == filterDto.ShopId);

            // Note: DriverName & ShopName filtering require joins or includes; assuming navigation properties exist

            // Fetch filtered orders
            var orders = await _orderRepo.FindAsync(predicate);

            // Sort newest to oldest (CreatedAt descending)
            orders = orders.OrderByDescending(o => o.CreatedAt).ToList();

            return _mapper.Map<List<OrderDto>>(orders);
        }

        // 2. Get orders belonging to specific user (company user)
        public async Task<List<OrderDto>> GetOrdersForUserAsync(IdentityUser currentUser)
        {
            int companyId = await GetCompanyIdAsync(currentUser);
            var orders = await _orderRepo.FindAsync(o => o.CompanyId == companyId);
            orders = orders.OrderByDescending(o => o.CreatedAt).ToList();
            return _mapper.Map<List<OrderDto>>(orders);
        }

        // 3. Add order
        public async Task<OrderDto> AddOrderAsync(OrderDto dto, IdentityUser currentUser)
        {
            int companyId = await GetCompanyIdAsync(currentUser);

            var order = _mapper.Map<Order>(dto);
            order.CompanyId = companyId;
            order.CreatedAt = DateTime.UtcNow;

            await _orderRepo.AddAsync(order);

            return _mapper.Map<OrderDto>(order);
        }

        // 4. Edit order
        public async Task<OrderDto> UpdateOrderAsync(OrderDto dto, IdentityUser currentUser)
        {
            int companyId = await GetCompanyIdAsync(currentUser);

            var order = await _orderRepo.GetByIdAsync(dto.OrderId);
            if (order == null || order.CompanyId != companyId)
                throw new UnauthorizedAccessException("You are not authorized to update this order.");

            // Update fields
            order.ShopId = dto.ShopId;
            order.CustomerName = dto.CustomerName;
            order.CustomerPhone = dto.CustomerPhone;
            order.CustomerAddress = dto.CustomerAddress;
            order.OrderAreaId = dto.OrderAreaId;
            order.OrderStatusId = dto.OrderStatusId;
            order.PaymentMethodId = dto.PaymentMethodId;
            order.Note = dto.Note;
            order.Amount = dto.Amount;
            order.TrackingPassword = dto.TrackingPassword;
            order.EstimatedArrivalTime = dto.EstimatedArrivalTime;
            order.IsCollectedFromCustomer = dto.IsCollectedFromCustomer;
            order.IsCollectedFromDriver = dto.IsCollectedFromDriver;
            order.IsPaidToShop = dto.IsPaidToShop;

            await _orderRepo.UpdateAsync(order);

            return _mapper.Map<OrderDto>(order);
        }

        // 5. Delete order (with authorization)
        public async Task DeleteOrderAsync(int orderId, IdentityUser currentUser)
        {
            int companyId = await GetCompanyIdAsync(currentUser);

            var order = await _orderRepo.GetByIdAsync(orderId);
            if (order == null || order.CompanyId != companyId)
                throw new UnauthorizedAccessException("You are not authorized to delete this order.");

            await _orderRepo.DeleteAsync(order);
        }
    }

    // Extension method for combining expressions with AND
    public static class ExpressionExtensions
    {
        public static Expression<Func<T, bool>> AndAlso<T>(
            this Expression<Func<T, bool>> expr1,
            Expression<Func<T, bool>> expr2)
        {
            var parameter = Expression.Parameter(typeof(T));

            var combined = new ReplaceParameterVisitor(expr1.Parameters[0], parameter).Visit(expr1.Body);
            var body2 = new ReplaceParameterVisitor(expr2.Parameters[0], parameter).Visit(expr2.Body);

            return Expression.Lambda<Func<T, bool>>(
                Expression.AndAlso(combined!, body2!),
                parameter);
        }

        private class ReplaceParameterVisitor : ExpressionVisitor
        {
            private readonly ParameterExpression _oldParam;
            private readonly ParameterExpression _newParam;

            public ReplaceParameterVisitor(ParameterExpression oldParam, ParameterExpression newParam)
            {
                _oldParam = oldParam;
                _newParam = newParam;
            }

            protected override Expression? VisitParameter(ParameterExpression node)
            {
                return node == _oldParam ? _newParam : base.VisitParameter(node);
            }
        }
    }
}
