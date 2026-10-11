using System;
using System.Collections.Generic;
using System.Text;

namespace LoanApplication.Application.DTO
{
    public class LoanProgressDTO
    {
        public int ApplicationId { get; set; }
        public string CurrentStatus { get; set; } = string.Empty;
        public List<ProgressStageDTO> Stages { get; set; } = new();
    }
}
