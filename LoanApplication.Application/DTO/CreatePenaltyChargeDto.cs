using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace LoanApplication.Application.DTO
{
    public class CreatePenaltyChargeDto
    {
        [Required]
        public int LoanAccountId { get; set; }

        [Required]
        public int EmiScheduleId { get; set; }

        [Required]
        [StringLength(50)]
        public string PenaltyType { get; set; }
    }
}
