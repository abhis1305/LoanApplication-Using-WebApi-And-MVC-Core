using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace LoanApplication.Application.DTO
{
    public class CreateDisbursementDto
    {
       
        public int DisbursementId { get; set; }
        public int DealId { get; set; }
        public decimal DisburseAmount { get; set; }

        public string BankPartner { get; set; } = string.Empty;
        public DateTime DisbursementDate { get; set; }

    }
}
