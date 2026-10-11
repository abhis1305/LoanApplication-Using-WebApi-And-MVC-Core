using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace LoanApplication.Application.DTO
{
    public class CreateTicketDTO
    {
        [Required]
        public int CustomerId { get; set; }

        [Required]
        public int LoanAccountId { get; set; }

        [Required, MaxLength(150)]
        public string Subject { get; set; } = string.Empty;

        [Required, MaxLength(2000)]
        public string Description { get; set; } = string.Empty;

        public string? AttachmentPath { get; set; }
    }
}
