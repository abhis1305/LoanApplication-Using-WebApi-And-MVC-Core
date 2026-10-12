using LoanApplication.Application.DTO;
using LoanApplication.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace LoanApplication.Application.Interfcae
{
    public interface IDisbursementService
    {
        Task<List<Disbursement>> GetAllAsync();

        Task<Disbursement?> GetByIdAsync(int id);

        Task<Disbursement> CreateAsync(CreateDisbursementDto dto);
    }
}
