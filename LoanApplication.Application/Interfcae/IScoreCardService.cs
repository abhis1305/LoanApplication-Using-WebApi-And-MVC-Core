using LoanApplication.Application.DTO;

namespace LoanApplication.Application.Interface
{
    public interface IScoreCardService
    {
        Task<ScoreCardResponseDto> GenerateScoreCardAsync(
            int customerId);
    }
}