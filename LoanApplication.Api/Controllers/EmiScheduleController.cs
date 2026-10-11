using LoanApplication.Application.Interfcae;
using LoanApplication.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LoanApplication.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmiScheduleController : ControllerBase
    {
        IEmiScheduleRepository service;
        public EmiScheduleController(IEmiScheduleRepository service)
        {
            this.service = service;
        }

        [HttpPost]
        [Route("AddEMI")]
        public async Task<IActionResult> AddEmiSchedule(EmiSchedule emi)
        {
            var d = await service.AddEmiSchedule(emi);
            return Ok(new { message = "EMI Schedule Added Successfully", data = d });
        }

        [HttpGet]
        [Route("GetEMI/{loanAccountId}")]
        public async Task<IActionResult> EmiScheduleList(int loanAccountId)
        {
            var d = await service.GetEmiSchedulesByLoanAccountId(loanAccountId);
            if (d == null || d.Count == 0)
            {
                return NotFound(new { message = "EMI Schedule Not Found" });
            }
            return Ok(d);
        }

        [HttpGet]
        [Route("GetEMIById/{id}")]
        public async Task<IActionResult> EmiScheduleById(int id)
        {
            var d = await service.GetEmiScheduleById(id);
            if (d == null)
            {
                return NotFound(new { message = "EMI Schedule Not Found" });
            }
            return Ok(d);
        }

        [HttpPut]
        [Route("UpdateEMI")]
        public async Task<IActionResult> UpdateEmiSchedule(EmiSchedule emi)
        {
            var d = await service.UpdateEmiSchedule(emi);
            if (d == null)
            {
                return NotFound(new { message = "EMI Schedule Not Found" });
            }
            return Ok(new { message = "EMI Schedule Updated Successfully", data = d });
        }
    }
}