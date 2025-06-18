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
        Task<NotificationDto> CreateAndSendAsync(NotificationDto dto, string userId);
        Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(string userId);
    }
}
