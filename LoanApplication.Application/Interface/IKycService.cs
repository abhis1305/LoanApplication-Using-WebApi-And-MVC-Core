using LoanApplication.Application.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace LoanApplication.Application.Interface
{
    public interface IKycService
    {
        

        Task<bool> UploadDocumentAsync(int userId,UploadKycDocumentDto dto);
        Task<List<KycDocumentResponseDto>> GetMyDocumentsAsync(int userId);
        Task<string?> GetMyDocumentFilePathAsync(int userId,int documentId);


        // Loan Officer APIs

        Task<List<KycCustomerListDto>> GetPendingCustomersAsync();
        Task<KycCustomerDetailsDto?> GetCustomerKycAsync(int customerId);
        Task<string?> GetOfficerDocumentFilePathAsync(int documentId);
        Task<bool> ApproveDocumentsAsync(int customerId,List<int> documentIds);
        Task<bool> RejectDocumentsAsync(int customerId,List<int> documentIds,string rejectionReason);
    }
}
