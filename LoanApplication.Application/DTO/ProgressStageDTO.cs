using System;
using System.Collections.Generic;
using System.Text;

namespace LoanApplication.Application.DTO
{
    public class ProgressStageDTO
    {
        public string Stage { get; set; } = string.Empty;
        public bool Done { get; set; }
        public DateTime? Date { get; set; }
    }
}
