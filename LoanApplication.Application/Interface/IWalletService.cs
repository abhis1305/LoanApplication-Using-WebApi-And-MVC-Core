
using LoanApplication.Application.DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LoanApplication.Application.Interface
{
    public interface IWalletService
    {
        Task<WalletResponseDTO> AddMoneyAsync(AddMoneyDTO dto);
        Task<WalletResponseDTO> CreateWalletAsync(int customerId);

        Task<WalletResponseDTO> GetBalanceAsync(int customerId);

        Task<WalletResponseDTO> MakePaymentAsync(WalletPaymentDTO dto);

        Task<WalletHistoryResponseDTO> GetTransactionsAsync(int customerId);
    }
}
