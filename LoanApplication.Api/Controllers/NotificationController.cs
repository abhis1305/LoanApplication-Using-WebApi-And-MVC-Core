using LoanApplication.Application.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LoanApplication.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        INotificationService service;

        public NotificationController(INotificationService service)
        {
            this.service = service;
        }

        [HttpGet("{customerId}")]
        public async Task<IActionResult> GetNotifications(int customerId)
        {
            var result = await service.GetNotifications(customerId);
            return Ok(result);
        }

        [HttpPut("MarkAsRead/{id}")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            await service.MarkAsRead(id);
            return Ok(new { message = "Notification marked as read" });
        }
    }
}
