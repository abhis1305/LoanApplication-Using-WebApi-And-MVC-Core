using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace LoanApplication.Application.DTO
{
    public class PayEmiDto
    {
        public int EmiScheduleId { get; set; }

        [Required]
        [StringLength(30)]
        public string PaymentMode { get; set; }
    }
}
