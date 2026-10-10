using LoanApp.Models;

namespace LoanApplication.Application.Interface;

public interface ICibilRepository
{
    Task<Customer?> GetCustomerByIdAsync(int customerId);

    Task<CibilReport> AddCibilReportAsync(CibilReport report);
}