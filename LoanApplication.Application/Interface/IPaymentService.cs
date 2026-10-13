using LoanApplication.Application.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace LoanApplication.Application.Interface
{
    public interface IPaymentService
    {
        Task<LoanPaymentDto> PayEmi(PayEmiDto dto, int userId);

        Task<LoanPaymentDto> PayPenalty(int penaltyId, int userId);

        Task<List<LoanPaymentDto>> GetMyPayments(int userId);
    }
}
