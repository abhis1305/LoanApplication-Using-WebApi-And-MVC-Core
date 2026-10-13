using System;
using System.Collections.Generic;
using System.Text;

namespace LoanApplication.Application.DTO
{
    public class SanctionLetterDocumentDto
    {
        public int SanctionId { get; set; }
        public int DealId { get; set; }
        public string FileName { get; set; } = string.Empty;
    }
}
