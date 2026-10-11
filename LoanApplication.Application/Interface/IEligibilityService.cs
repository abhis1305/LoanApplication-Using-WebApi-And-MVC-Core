using LoanApplication.Application.DTO;

namespace LoanApplication.Application.Interface
{
    public interface IEligibilityService
    {
        Task<EligibilityResponseDto> CheckEligibilityAsync(int customerId);

        Task<List<EligibilityResponseDto>> GetPendingRequestsAsync();

        Task<EligibilityResponseDto> UpdateOfficerDecisionAsync(int eligibilityId,bool approved,string? remarks);

        Task<EligibilityResponseDto> GetEligibilityResultAsync(int eligibilityId);
    }
}