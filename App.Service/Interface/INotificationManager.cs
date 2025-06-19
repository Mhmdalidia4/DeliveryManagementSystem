using App.Domain.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Service.Interface
{
    public interface INotificationManager
    {
        Task<NotificationDto> CreateAsync(NotificationDto dto);
        Task<NotificationDto> CreateAndSendAsync(NotificationDto dto, string userId);
        Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(string userId);
        Task<IEnumerable<NotificationDto>> GetAllByUserAsync(string userId);
        Task<IEnumerable<NotificationDto>> GetUnreadByUserAsync(string userId);
        Task MarkAsReadAsync(int notificationId, string userId);
        Task MarkAllAsReadAsync(string userId);
        Task DeleteAsync(int notificationId, string userId);
    }
}
