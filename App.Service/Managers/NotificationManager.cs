//using App.Domain.DTOs;
//using App.Domain.Interfaces;
//using App.Domain.Models;
//using App.Service.Hubs;
//using App.Service.Interface;
//using AutoMapper;
//using Microsoft.AspNetCore.SignalR;

//public class NotificationManager: INotificationManager
//{
//    private readonly INotificationRepository _notificationRepository;
//    private readonly IMapper _mapper;
//    //private readonly IHubContext<NotificationHub> _hubContext;

//    public NotificationManager(
//        INotificationRepository notificationRepository,
//        IMapper mapper,
//        //IHubContext<NotificationHub> hubContext)
//    {
//        _notificationRepository = notificationRepository;
//        _mapper = mapper;
//        //_hubContext = hubContext;
//    }

//    public async Task<NotificationDto> CreateAndSendAsync(NotificationDto dto, string userId)
//    {
//        // Map DTO to entity
//        var notification = _mapper.Map<Notification>(dto);

//        // Save to database
//        await _notificationRepository.AddAsync(notification);

//        // Optionally map back to DTO (with ID, etc.)
//        var resultDto = _mapper.Map<NotificationDto>(notification);

//        // Send notification via SignalR (calls the client-side "ReceiveNotification" method)
//        //await _hubContext.Clients.User(userId).SendAsync("ReceiveNotification", resultDto.Message);

//        return resultDto;
//    }

//    public async Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(string userId)
//    {
//        var userNotifications = await _notificationRepository.FindAsync(n => n.UserId == userId);
//        return _mapper.Map<IEnumerable<NotificationDto>>(userNotifications);
//    }
//}
