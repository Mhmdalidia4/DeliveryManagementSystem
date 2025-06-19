using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using App.Service.Interface;
using App.Domain.DTOs;

namespace App.Web.Controllers
{
    [Authorize]
    [Route("[controller]")]
    public class NotificationsController : Controller
    {
        private readonly INotificationManager _notificationManager;
        private readonly UserManager<IdentityUser> _userManager;

        public NotificationsController(
            INotificationManager notificationManager,
            UserManager<IdentityUser> userManager)
        {
            _notificationManager = notificationManager;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var notifications = await _notificationManager.GetAllByUserAsync(user.Id);
            return View(notifications);
        }

        [HttpGet("GetUnreadNotifications")]
        public async Task<IActionResult> GetUnreadNotifications()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Unauthorized();

                var notifications = await _notificationManager.GetUnreadByUserAsync(user.Id);
                
                // Map to a simple object for JSON serialization
                var result = notifications.Select(n => new
                {
                    id = n.NotificationId,
                    title = n.Title ?? "Notification",
                    message = n.Message ?? "",
                    createdAt = n.CreatedAt?.ToString("yyyy-MM-ddTHH:mm:ss"),
                    isRead = n.IsRead ?? false,
                    type = n.Type ?? "info"
                }).ToList();

                return Json(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("MarkAsRead")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Unauthorized();

                await _notificationManager.MarkAsReadAsync(id, user.Id);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("MarkAllAsRead")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAllAsRead()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Unauthorized();

                await _notificationManager.MarkAllAsReadAsync(user.Id);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromBody] NotificationDto notification)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Unauthorized();

                // For demo purposes - in real scenario this would be called from business logic
                notification.UserId = user.Id;
                notification.CreatedAt = DateTime.Now;
                notification.IsRead = false;

                await _notificationManager.CreateAsync(notification);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("CreateTest")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTest()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Unauthorized();

                // Create a test notification
                var notification = new NotificationDto
                {
                    UserId = user.Id,
                    Title = "Test Notification",
                    Message = "This is a test notification to verify the system is working correctly.",
                    Type = "info",
                    CreatedAt = DateTime.Now,
                    IsRead = false
                };

                await _notificationManager.CreateAndSendAsync(notification, user.Id);
                return Json(new { success = true, message = "Test notification created!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Unauthorized();

                await _notificationManager.DeleteAsync(id, user.Id);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
