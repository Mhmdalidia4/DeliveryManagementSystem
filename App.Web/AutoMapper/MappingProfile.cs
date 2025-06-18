using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using App.Domain.DTOs;
using App.Domain.Models;
using App.Web.ViewModels;
using AutoMapper;
namespace App.Service.AutoMapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Company
            CreateMap<Company, CompanyDto>().ReverseMap();

            // Shop
            CreateMap<Shop, ShopDto>().ReverseMap();

            // Driver
            CreateMap<Driver, DriverDto>().ReverseMap();

            // Order
            CreateMap<Order, OrderDto>().ReverseMap();

            // OrderArea
            CreateMap<OrderArea, OrderAreaDto>().ReverseMap();

            // OrderStatus
            CreateMap<OrderStatus, OrderStatusDto>().ReverseMap();

            // PaymentMethod
            CreateMap<PaymentMethod, PaymentMethodDto>().ReverseMap();

            // DriverAssignment
            CreateMap<DriverAssignment, DriverAssignmentDto>().ReverseMap();

            // Feedback
            CreateMap<Feedback, FeedbackDto>().ReverseMap();

            // Notification
            CreateMap<Notification, NotificationDto>().ReverseMap();

            // OrdersToReview
            CreateMap<OrdersToReview, OrdersToReviewDto>().ReverseMap();

            // PickupRequest → DTO
            CreateMap<PickUpRequest, PickupRequestDto>().ReverseMap();

            // PickupRequest → ViewModel (for display purposes)
            CreateMap<PickUpRequest, PickupRequestViewModel>()
                .ForMember(dest => dest.ShopName, opt => opt.MapFrom(src => src.Shop.Name))
                .ForMember(dest => dest.DriverName, opt => opt.MapFrom(src => src.AssignedDriver.Name));
        }
    }
}
