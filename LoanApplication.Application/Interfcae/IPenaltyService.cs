using LoanApplication.Application.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace LoanApplication.Application.Interfcae
{
    public interface IPenaltyService
    {
        Task<List<PenaltyChargeDto>> GetAllPenalties();
        Task<PenaltyChargeDto?> GetPenaltyById(int penaltyId);
        Task<List<PenaltyChargeDto>> GetPenaltiesByLoanAccountId(int loanAccountId);
        Task ApplyPenalty(CreatePenaltyChargeDto dto);
        Task WaivePenalty(int penaltyId, WaivePenaltyDto dto);
    }
}
