using LoanApplication.Application.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace LoanApplication.Application.Interface
{
    public interface ILoanDealService
    {
        Task<LoanDealDto> ApplyLoanAsync(CreateLoanDealDto dto);

        Task<LoanCalculationDto> CalculateLoanAsync(int customerId, decimal loanAmount, int tenureYears);

        Task<List<LoanDealDto>> GetPendingLoansAsync();

        Task<LoanDealDto?> GetLoanByIdAsync(int dealId);

        Task<bool> ApproveLoanAsync(int dealId, LoanDealReviewDto dto);

        Task<bool> RejectLoanAsync(int dealId, LoanDealReviewDto dto);

    }
}
