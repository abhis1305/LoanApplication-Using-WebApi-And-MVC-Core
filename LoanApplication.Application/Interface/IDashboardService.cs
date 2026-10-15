using LoanApplication.Application.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace LoanApplication.Application.Interfcae
{
    public interface IDashboardService
    {
        Task<DashboardCardsDTO> FetchCardsAsync(int customerId);
        Task<LoanProgressDTO?> FetchProgressAsync(int customerId);
        Task<ActiveLoanDTO?> FetchActiveLoanAsync(int customerId);
        Task<EmiSummaryDTO?> FetchEmiSummaryAsync(int customerId);
        Task<List<RecentPaymentDTO>> FetchRecentPaymentsAsync(int customerId);
        Task<List<DocumentDTO>> FetchDocumentsAsync(int customerId);
    }
}
