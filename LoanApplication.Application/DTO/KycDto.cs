using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace LoanApplication.Application.DTO
{
    public class UploadKycDocumentDto
    {
        [Required]
        public string DocumentType { get; set; } = string.Empty;
        [Required]
        public IFormFile File { get; set; } = null!;
    }

    // Document details returned to customer
    public class KycDocumentResponseDto
    {
        public int DocumentId { get; set; }
        public string DocumentType { get; set; } = string.Empty;
        public string VerificationStatus { get; set; } = string.Empty;
        public string? RejectionReason { get; set; }
    }

    // Officer's customer list
    public class KycCustomerListDto
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string MobileNo { get; set; } = string.Empty;
    }

    // Officer opens View KYC
    public class KycCustomerDetailsDto
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public List<KycDocumentResponseDto> Documents { get; set; } = new();
    }

    // Officer approves selected documents
    public class KycDocumentDecisionDto
    {
        [Required]
        public List<int> DocumentIds { get; set; } = new();
    }

    // Officer rejects selected documents
    public class RejectKycDocumentsDto
    {
        [Required]
        public List<int> DocumentIds { get; set; } = new();

        [Required]
        [StringLength(500)]
        public string RejectionReason { get; set; } = string.Empty;
    }
}
