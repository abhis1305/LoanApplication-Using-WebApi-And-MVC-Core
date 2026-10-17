using LoanApplication.Application.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace LoanApplication.Application.Interface
{
    public interface ICustomerProfileService
    {
        Task<CustomerProfileDto?> GetMyProfileAsync(string identity,CancellationToken cancellationToken = default);
        Task<CustomerProfileDto?> UpdateMyProfileAsync(string identity,UpdateCustomerProfileDto dto,CancellationToken cancellationToken = default);
    }
}
