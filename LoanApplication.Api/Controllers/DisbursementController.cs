using LoanApplication.Application.DTO;
using LoanApplication.Application.Interfcae;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LoanApplication.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DisbursementController : ControllerBase
    {

        private readonly IDisbursementService service;

        public DisbursementController( IDisbursementService service)
        {
            this.service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await service.GetAllAsync());
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await service.GetByIdAsync(id);

            if (data == null)
                return NotFound(new
                {
                    message = "Disbursement not found."
                });

            return Ok(data);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateDisbursementDto dto)
        {
            try
            {
                var data = await service.CreateAsync(dto);

                return Ok(data);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }
    }
}

