
using LoanApplication.Application.DTO;
using LoanApplication.Application.Interface;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LoanApplication.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WalletController : ControllerBase
    {
        IWalletService service;

        public WalletController(IWalletService service)
        {
            this.service = service;
        }


        [HttpPost]
        [Route("Create/{customerId}")]
        public async Task<IActionResult> CreateWallet(int customerId)
        {
            try
            {
                var data = await service.CreateWalletAsync(customerId);
                return Ok(new { message = data.Message, data = data });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost]
        [Route("AddMoney")]
        public async Task<IActionResult> AddMoney(AddMoneyDTO dto)
        {
            try
            {
                var data = await service.AddMoneyAsync(dto);
                return Ok(new { message = data.Message, data = data });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        [Route("Balance/{customerId}")]
        public async Task<IActionResult> GetBalance(int customerId)
        {
            try
            {
                var data = await service.GetBalanceAsync(customerId);
                return Ok(data);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost]
        [Route("Pay")]
        public async Task<IActionResult> MakePayment(WalletPaymentDTO dto)
        {
            try
            {
                var data = await service.MakePaymentAsync(dto);
                return Ok(new { message = data.Message, data = data });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        [Route("Transactions/{customerId}")]
        public async Task<IActionResult> GetTransactions(int customerId)
        {
            try
            {
                var data = await service.GetTransactionsAsync(customerId);
                return Ok(data);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
