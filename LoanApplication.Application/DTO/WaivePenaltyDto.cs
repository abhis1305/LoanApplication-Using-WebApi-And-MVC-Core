using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace LoanApplication.Application.DTO
{
    public class WaivePenaltyDto
    {
        [Required]
        [StringLength(250, MinimumLength = 5)]
        public string Reason { get; set; }
    }
}
