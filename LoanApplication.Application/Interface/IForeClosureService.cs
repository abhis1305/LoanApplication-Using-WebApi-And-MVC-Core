using LoanApplication.Application.DTO;
using LoanApplication.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace LoanApplication.Application.Interface
{
    public interface IForeClosureService
    {
        Task<ForeClosureRequest> CreateRequest(CreateForeClosureRequestDto dto, int userId);

        Task<List<ForeClosureRequest>> GetMyRequests(int userId);

        Task<List<ForeClosureRequest>> GetAllRequests();
        Task<ForeClosureRequest?> GetRequestById(int requestId);
        Task<ForeClosureRequest> ApproveRequest(int requestId);
        Task<ForeClosureRequest> RejectRequest(int requestId);
    }
}
