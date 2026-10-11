using LoanApplication.Application.DTO;
using LoanApplication.Application.Interfcae;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LoanApplication.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SupportTicketController : ControllerBase
    {
        ISupportTicketService service;
        public SupportTicketController(ISupportTicketService service)
        {
            this.service = service;
        }

        [HttpPost]
        [Route("AddT")]
        public async Task<IActionResult> AddTicket(CreateTicketDTO t)
        {
            await service.AddTicketAsync(t);
            return Ok(new { message = "Ticket Raised Success" });
        }

        [HttpGet]
        [Route("GetT")]
        public async Task<IActionResult> TicketList()
        {
            var d = await service.FetchTicketsAsync();
            return Ok(d);
        }

        [HttpGet]
        [Route("MyT/{customerId}")]
        public async Task<IActionResult> MyTicketList(int customerId)
        {
            var d = await service.FetchMyTicketsAsync(customerId);
            return Ok(d);
        }

        [HttpGet]
        [Route("GetT/{id}")]
        public async Task<IActionResult> TicketById(int id)
        {
            var d = await service.FetchTicketByIdAsync(id);
            if (d == null)
                return NotFound(new { message = "Ticket Not Found" });
            return Ok(d);
        }

        [HttpPut]
        [Route("RespondT/{id}")]
        public async Task<IActionResult> RespondTicket(int id, OfficerResponseDTO dto)
        {
            var done = await service.RespondTicketAsync(id, dto);
            if (!done)
                return NotFound(new { message = "Ticket Not Found" });
            return Ok(new { message = "Response Saved Success" });
        }

        [HttpDelete]
        [Route("DeleteT/{id}")]
        public async Task<IActionResult> DeleteTicket(int id)
        {
            var done = await service.DeleteTicketAsync(id);
            if (!done)
                return NotFound(new { message = "Ticket Not Found" });
            return Ok(new { message = "Ticket Deleted Success" });
        }
    }
}