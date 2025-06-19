using App.Domain.DTOs;
using App.Domain.Interfaces;
using App.Domain.Models;
using App.Service.Hubs;
using App.Service.Interface;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;

namespace App.Service.Managers
{
    public class NotificationManager : INotificationManager
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IMapper _mapper;
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationManager(
            INotificationRepository notificationRepository,
            IMapper mapper,
            IHubContext<NotificationHub> hubContext)
        {
            _notificationRepository = notificationRepository;
            _mapper = mapper;
            _hubContext = hubContext;
        }

        public async Task<NotificationDto> CreateAsync(NotificationDto dto)
        {
            var notification = _mapper.Map<Notification>(dto);
            await _notificationRepository.AddAsync(notification);
            return _mapper.Map<NotificationDto>(notification);
        }

        public async Task<NotificationDto> CreateAndSendAsync(NotificationDto dto, string userId)
        {
            // Map DTO to entity
            var notification = _mapper.Map<Notification>(dto);
            notification.UserId = userId;
            notification.CreatedAt = DateTime.Now;
            notification.IsRead = false;

            // Save to database
            await _notificationRepository.AddAsync(notification);

            // Map back to DTO
            var resultDto = _mapper.Map<NotificationDto>(notification);

            // Send notification via SignalR
            await _hubContext.Clients.User(userId).SendAsync("ReceiveNotification", resultDto);

            return resultDto;
        }

        public async Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(string userId)
        {
            var userNotifications = await _notificationRepository.FindAsync(n => n.UserId == userId);
            return _mapper.Map<IEnumerable<NotificationDto>>(userNotifications.OrderByDescending(n => n.CreatedAt));
        }

        public async Task<IEnumerable<NotificationDto>> GetAllByUserAsync(string userId)
        {
            return await GetUserNotificationsAsync(userId);
        }

        public async Task<IEnumerable<NotificationDto>> GetUnreadByUserAsync(string userId)
        {
            var userNotifications = await _notificationRepository.FindAsync(n => n.UserId == userId && n.IsRead == false);
            return _mapper.Map<IEnumerable<NotificationDto>>(userNotifications.OrderByDescending(n => n.CreatedAt));
        }

        public async Task MarkAsReadAsync(int notificationId, string userId)
        {
            var notification = await _notificationRepository.GetByIdAsync(notificationId);
            if (notification != null && notification.UserId == userId)
            {
                notification.IsRead = true;
                await _notificationRepository.UpdateAsync(notification);
            }
        }

        public async Task MarkAllAsReadAsync(string userId)
        {
            var userNotifications = await _notificationRepository.FindAsync(n => n.UserId == userId && n.IsRead == false);
            foreach (var notification in userNotifications)
            {
                notification.IsRead = true;
                await _notificationRepository.UpdateAsync(notification);
            }
        }

        public async Task DeleteAsync(int notificationId, string userId)
        {
            var notification = await _notificationRepository.GetByIdAsync(notificationId);
            if (notification != null && notification.UserId == userId)
            {
                await _notificationRepository.DeleteAsync(notification);
            }
        }
    }
}
