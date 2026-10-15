using LoanApplication.Application.DTO;

namespace LoanApplication.Application.Interface
{
    public interface ICibilService
    {
        Task<CibilScoreResponseDto> GenerateScoreAsync(int customerId);

        Task<CibilScoreResponseDto> GetCibilScoreAsync(int customerId);
    }
}