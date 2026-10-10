namespace LoanApplication.Application.DTO;

public class CibilScoreResponseDto
{
    public int CibilReportId { get; set; }

    public int CustomerId { get; set; }

    public string PanNo { get; set; } = string.Empty;

    public int IncomeScore { get; set; }

    public int EmploymentScore { get; set; }

    public int AgeScore { get; set; }

    public decimal FOIR { get; set; }

    public int FOIRScore { get; set; }

    public int CibilScore { get; set; }

    public string CibilStatus { get; set; } = string.Empty;

    public DateTime CheckDate { get; set; }
}