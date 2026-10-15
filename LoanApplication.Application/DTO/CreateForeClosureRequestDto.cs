using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace LoanApplication.Application.DTO
{
    public class CreateForeClosureRequestDto
    {
        public int LoanAccountId { get; set; }

        [Required]

        public string ForeClosureType { get; set; } = string.Empty;

        public decimal PartialAmount { get; set; }

        [Required]
        public DateTime ExpectedClosureDate { get; set; }

        [Required]
        public string Reason { get; set; } 
    }
}
