using LoanApplication.Application.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace LoanApplication.Application.Interface
{
    public interface ISanctionLetterService
    {
        Task<List<ApprovedLoanDto>> GetApprovedLoansAsync();

        Task<SanctionLetterDocumentDto> GenerateSanctionLetterAsync(int dealId);

        Task<SanctionLetterDataDto?> GetSanctionLetterDataAsync(int dealId);

        Task<SanctionLetterDocumentDto?> GetSanctionLetterAsync(int dealId);
    }
}
