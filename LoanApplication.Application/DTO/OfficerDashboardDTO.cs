using System;
using System.Collections.Generic;
using System.Text;

namespace LoanApplication.Application.DTO
{
    public class OfficerDashboardDTO
    {
        public LosCardsDTO LosCards { get; set; } = new();
        public LmsCardsDTO LmsCards { get; set; } = new();
        public List<FunnelStageDTO> Funnel { get; set; } = new();                    // LOS: application funnel
        public List<PortfolioRowDTO> PortfolioSummary { get; set; } = new();         // LMS: portfolio donut
        public List<CollectionMonthDTO> CollectionEfficiency { get; set; } = new();  // LMS: monthly bars
    }
}
