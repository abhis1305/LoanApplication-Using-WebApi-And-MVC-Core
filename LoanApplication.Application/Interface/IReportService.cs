using LoanApplication.Application.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace LoanApplication.Application.Interface
{
    public interface IReportService
    {
        // los
        Task<LosCardsDTO> FetchLosCardsAsync();
        Task<List<ProductWiseRowDTO>> FetchProductWiseAsync(DateTime? from, DateTime? to);
        Task<List<TATRowDTO>> FetchTatAsync(DateTime? from, DateTime? to);
        Task<List<CibilRangeRowDTO>> FetchCibilRangeAsync(DateTime? from, DateTime? to);
        Task<List<ScoreCardRowDTO>> FetchScoreCardWiseAsync(DateTime? from, DateTime? to);
        Task<List<FunnelStageDTO>> FetchFunnelAsync(DateTime? from, DateTime? to);
        Task<List<CollectionMonthDTO>> FetchCollectionEfficiencyAsync(DateTime? from, DateTime? to);

        // lms
        Task<LmsCardsDTO> FetchLmsCardsAsync();
        Task<List<PortfolioRowDTO>> FetchPortfolioAsync();
        Task<EmiCollectionDTO> FetchEmiCollectionAsync(DateTime? from, DateTime? to);
        Task<List<BucketRowDTO>> FetchOverdueAsync();
        Task<List<DisbursementRowDTO>> FetchDisbursementAsync(DateTime? from, DateTime? to);
        Task<List<PrepaymentRowDTO>> FetchPrepaymentAsync(DateTime? from, DateTime? to);
    }
}
