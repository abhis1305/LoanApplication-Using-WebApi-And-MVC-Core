using LoanApplication.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace LoanApplication.Application.Interfcae
{
    public interface IEmiScheduleRepository
    {
        Task<EmiSchedule> AddEmiSchedule(EmiSchedule emiSchedule);
        Task AddEmiSchedule(int loanAccountId);
        Task<List<EmiSchedule>> GetEmiSchedulesByLoanAccountId(int loanAccountId);
        Task<EmiSchedule> UpdateEmiSchedule(EmiSchedule emiSchedule);
        Task<EmiSchedule> GetEmiScheduleById(int emiScheduleId);
    }
}
