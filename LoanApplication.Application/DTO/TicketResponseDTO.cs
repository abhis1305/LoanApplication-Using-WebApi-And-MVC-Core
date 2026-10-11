using System;
using System.Collections.Generic;
using System.Text;

namespace LoanApplication.Application.DTO
{
    public class TicketResponseDTO
    {
        public int TicketId { get; set; }
        public string LoanAccountNo { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? AttachmentPath { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public string? OfficerResponse { get; set; }
    }
}
