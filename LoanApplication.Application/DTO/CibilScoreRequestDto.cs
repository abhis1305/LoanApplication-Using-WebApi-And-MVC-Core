using System.ComponentModel.DataAnnotations;

namespace LoanApplication.Application.DTO;

public class CibilScoreRequestDto
{
    [Range(1, int.MaxValue)]
    public int CustomerId { get; set; }
}