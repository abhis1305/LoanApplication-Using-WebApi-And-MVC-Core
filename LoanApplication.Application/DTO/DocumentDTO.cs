using System;
using System.Collections.Generic;
using System.Text;

namespace LoanApplication.Application.DTO
{
    public class DocumentDTO
    {
        public int DocumentId { get; set; }
        public string DocumentType { get; set; } = string.Empty;
        public string VerificationStatus { get; set; } = string.Empty;
    }
}
