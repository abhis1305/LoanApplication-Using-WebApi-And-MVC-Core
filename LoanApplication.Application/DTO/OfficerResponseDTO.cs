using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace LoanApplication.Application.DTO
{
    public class OfficerResponseDTO
    {
        [Required, MaxLength(2000)]
        public string OfficerResponse { get; set; } = string.Empty;

        public string Status { get; set; } = "Completed";
    }
}
