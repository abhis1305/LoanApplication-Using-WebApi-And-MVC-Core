using LoanApplication.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace LoanApplication.Application.Interfcae
{
    public interface ILoanAccountRepository
    {
        Task<LoanAccount> AddLoanAccount(LoanAccount loanAccount);
        Task<LoanAccount> CreateLoanAccount(int dealId);
        Task<LoanAccount> UpdateLoanAccount(LoanAccount loanAccount);
        Task<LoanAccount> GetLoanAccountById(int loanAccountId);
        Task<LoanAccount> GetByLoanDealId(int loanDealId);
        Task<List<LoanAccount>> GetAllLoanAccounts();
    }
}
