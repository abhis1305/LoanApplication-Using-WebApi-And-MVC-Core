using LoanApplication.Application.DTO;

namespace LoanApplication.Application.Interface
{
    public interface ICibilService
    {
        Task<CibilScoreResponseDto> GenerateScoreAsync(int customerId);
        Task<CibilCustomerProfileDto?> GetCustomerProfileAsync(int userId);

        Task<CibilScoreResponseDto> GetCibilScoreAsync(int customerId);
    }
}