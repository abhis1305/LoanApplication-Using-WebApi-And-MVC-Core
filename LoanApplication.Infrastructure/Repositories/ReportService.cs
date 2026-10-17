using LoanApplication.Application.DTO;
using LoanApplication.Application.Interface;
using LoanApplication.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace LoanApplication.Infrastructure.Repositories
{
    public class ReportService : IReportService
    {
        AppDBContext db;
        const string StatusApproved = "Approved";
        const string StatusRejected = "Rejected";
        const string StatusPending = "Pending";
        const string StatusDisbursed = "Disbursed";
        const string LoanActive = "Active";
        const string LoanClosed = "Closed";
        const string EmiPaid = "Paid";

        public ReportService(AppDBContext db)
        {
            this.db = db;
        }

        private static (DateTime from, DateTime to) Range(DateTime? from, DateTime? to)
        {
            var f = (from ?? new DateTime(DateTime.Today.Year, 1, 1)).Date;
            var t = (to ?? DateTime.Today).Date.AddDays(1).AddTicks(-1);
            return (f, t);
        }
        public async Task<List<DisbursementRowDTO>> FetchDisbursementAsync(DateTime? from, DateTime? to)
        {
            var (f, t) = Range(from, to);
            var data = await db.Disbursements
                .Where(d => d.DisbursementDate >= f && d.DisbursementDate <= t)
                .GroupBy(d => new { d.DisbursementDate.Year, d.DisbursementDate.Month })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    Loans = g.Count(),
                    Amount = g.Sum(x => x.DisburseAmount)
                })
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .ToListAsync();

            return data.Select(x => new DisbursementRowDTO
            {
                Month = x.Year + "-" + x.Month.ToString("00"),
                Loans = x.Loans,
                Amount = x.Amount
            }).ToList();
        }

        public async Task<EmiCollectionDTO> FetchEmiCollectionAsync(DateTime? from, DateTime? to)
        {
            var (f, t) = Range(from, to);
            var emis = db.EmiSchedules.Where(e => e.DueDate >= f && e.DueDate <= t);

            var billedCount = await emis.CountAsync();
            var billedAmount = await emis.SumAsync(e => (decimal?)e.Emi) ?? 0;

            var paid = emis.Where(e => e.PaymentStatus == EmiPaid);
            var collectedCount = await paid.CountAsync();
            var collectedAmount = await paid.SumAsync(e => (decimal?)e.Emi) ?? 0;

            return new EmiCollectionDTO
            {
                BilledCount = billedCount,
                BilledAmount = billedAmount,
                CollectedCount = collectedCount,
                CollectedAmount = collectedAmount,
                PendingCount = billedCount - collectedCount,
                PendingAmount = billedAmount - collectedAmount,
                EfficiencyPercent = billedAmount > 0
                    ? Math.Round((double)(collectedAmount * 100 / billedAmount), 2) : 0
            };
        }

        public async Task<LmsCardsDTO> FetchLmsCardsAsync()
        {
            return new LmsCardsDTO
            {
                TotalLoans = await db.LoanAccounts.CountAsync(),
                ActiveLoans = await db.LoanAccounts.CountAsync(l => l.LoanStatus == LoanActive),
                ClosedLoans = await db.LoanAccounts.CountAsync(l => l.LoanStatus == LoanClosed)
            };
        }

        public async Task<LosCardsDTO> FetchLosCardsAsync()
        {
            return new LosCardsDTO
            {
                TotalApplications = await db.LoanDeals.CountAsync(),
                Approved = await db.LoanDeals.CountAsync(d =>
                    d.CurrentStatus == StatusApproved || d.CurrentStatus == StatusDisbursed),
                Rejected = await db.LoanDeals.CountAsync(d => d.CurrentStatus == StatusRejected),
                Pending = await db.LoanDeals.CountAsync(d => d.CurrentStatus == StatusPending)
            };
        }

        public async Task<List<BucketRowDTO>> FetchOverdueAsync()
        {
            var today = DateTime.Today;
            var rows = await db.EmiSchedules
                .Where(e => e.PaymentStatus != EmiPaid && e.DueDate < today)
                .GroupBy(e => e.LoanAccountId)
                .Select(g => new
                {
                    OldestDue = g.Min(x => x.DueDate),
                    Amount = g.Sum(x => x.Emi)
                })
                .ToListAsync();

            var accounts = rows
                .Select(r => new { Days = (today - r.OldestDue).Days, r.Amount })
                .ToList();

            var buckets = new (string name, int min, int max)[]
            {
                ("1 - 30 Days", 1, 30),
                ("31 - 60 Days", 31, 60),
                ("61 - 90 Days", 61, 90),
                ("Above 90 Days", 91, int.MaxValue)
            };

            var result = buckets.Select(b =>
            {
                var sel = accounts.Where(a => a.Days >= b.min && a.Days <= b.max).ToList();
                return new BucketRowDTO
                {
                    Bucket = b.name,
                    Accounts = sel.Count,
                    Amount = sel.Sum(a => a.Amount)
                };
            }).ToList();

            var total = result.Sum(r => r.Amount);
            foreach (var r in result)
                r.Percent = total > 0 ? Math.Round((double)(r.Amount * 100 / total), 2) : 0;
            return result;
        }

        public async Task<List<PortfolioRowDTO>> FetchPortfolioAsync()
        {
            var data = await db.LoanAccounts
                .Where(l => l.LoanStatus == LoanActive)
                .Join(db.LoanDeals, l => l.DealId, d => d.DealId,
                    (l, d) => new { d.LoanType, l.OutstandingPrincipal })
                .GroupBy(x => x.LoanType)
                .Select(g => new PortfolioRowDTO
                {
                    Product = g.Key,
                    ActiveLoans = g.Count(),
                    Outstanding = g.Sum(x => x.OutstandingPrincipal)
                })
                .ToListAsync();

            var total = data.Sum(x => x.Outstanding);
            foreach (var r in data)
                r.Percent = total > 0 ? Math.Round((double)(r.Outstanding * 100 / total), 2) : 0;
            return data;
        }

        public async Task<List<PrepaymentRowDTO>> FetchPrepaymentAsync(DateTime? from, DateTime? to)
        {
            var (f, t) = Range(from, to);
            return await db.LoanClosures
                .Where(c => c.ClosureDate >= f && c.ClosureDate <= t)
                .GroupBy(c => c.ClosureType)
                .Select(g => new PrepaymentRowDTO
                {
                    Type = g.Key,
                    Accounts = g.Count(),
                    Amount = g.Sum(x => x.FinalSettlementAmount)
                })
                .ToListAsync();
        }

        public async Task<List<ProductWiseRowDTO>> FetchProductWiseAsync(DateTime? from, DateTime? to)
        {
            var (f, t) = Range(from, to);
            var data = await db.LoanDeals
                .Where(d => d.AppliedDate >= f && d.AppliedDate <= t)
                .GroupBy(d => d.LoanType)
                .Select(g => new ProductWiseRowDTO
                {
                    Product = g.Key,
                    TotalApps = g.Count(),
                    Approved = g.Count(d => d.CurrentStatus == StatusApproved || d.CurrentStatus == StatusDisbursed),
                    Disbursed = g.Count(d => d.CurrentStatus == StatusDisbursed)
                })
                .ToListAsync();

            foreach (var r in data)
                r.DisbursementPercent = r.TotalApps > 0 ? Math.Round(r.Disbursed * 100.0 / r.TotalApps, 2) : 0;
            return data;
        }

        public async Task<List<TATRowDTO>> FetchTatAsync(DateTime? from, DateTime? to)
        {
            var (f, t) = Range(from, to);
            var rows = await db.LoanDeals
                .Where(d => d.AppliedDate >= f && d.AppliedDate <= t)
                .Select(d => new
                {
                    d.AppliedDate,
                    Sanction = d.SanctionLetters!.Min(s => (DateTime?)s.CreatedAt),
                    Disbursed = d.Disbursements!.Min(x => (DateTime?)x.DisbursementDate)
                })
                .ToListAsync();

            var applyToSanction = rows.Where(r => r.Sanction != null)
                .Select(r => (r.Sanction!.Value - r.AppliedDate).TotalDays).ToList();
            var sanctionToDisburse = rows.Where(r => r.Sanction != null && r.Disbursed != null)
                .Select(r => (r.Disbursed!.Value - r.Sanction!.Value).TotalDays).ToList();
            var applyToDisburse = rows.Where(r => r.Disbursed != null)
                .Select(r => (r.Disbursed!.Value - r.AppliedDate).TotalDays).ToList();

            return new List<TATRowDTO>
            {
                MakeTat("Application to Sanction", applyToSanction),
                MakeTat("Sanction to Disbursement", sanctionToDisburse),
                MakeTat("Application to Disbursement (Total)", applyToDisburse)
            };
        }

        private static TATRowDTO MakeTat(string stage, List<double> days)
        {
            return new TATRowDTO
            {
                Stage = stage,
                Count = days.Count,
                AvgDays = days.Count > 0 ? Math.Round(days.Average(), 1) : 0,
                MinDays = days.Count > 0 ? Math.Round(days.Min(), 1) : 0,
                MaxDays = days.Count > 0 ? Math.Round(days.Max(), 1) : 0
            };
        }

        private static readonly (string Status, string Range, int Min, int Max, string Decision)[] ScoreBands =
        {
        ("Excellent", "900 +",      900, int.MaxValue, "Auto Approved"),
        ("Very Good", "800 - 899",  800, 899,          "Auto Approved"),
        ("Good",      "750 - 799",  750, 799,          "Manual Review"),
        ("Average",   "700 - 749",  700, 749,          "Manual Review"),
        ("Risky",     "650 - 699",  650, 699,          "Manual Review"),
        ("Reject",    "Below 650",  int.MinValue, 649, "Auto Reject")
        };

        private async Task<List<(int Score, string Status)>> GetScoredApplicationsAsync(DateTime? from, DateTime? to)
        {
            var (f, t) = Range(from, to);

            // latest eligibility score per customer (small table, done in memory)
            var eligibility = await db.EligibilityResults
                .Select(e => new { e.CustomerId, e.EligibilityId, e.CibilScore })
                .ToListAsync();
            var scoreByCustomer = eligibility
                .GroupBy(e => e.CustomerId)
                .ToDictionary(g => g.Key, g => (int)g.OrderByDescending(x => x.EligibilityId).First().CibilScore);

            var deals = await db.LoanDeals
                .Where(d => d.AppliedDate >= f && d.AppliedDate <= t)
                .Select(d => new { d.CustomerId, d.CurrentStatus })
                .ToListAsync();

            return deals
                .Where(d => scoreByCustomer.ContainsKey(d.CustomerId))
                .Select(d => (scoreByCustomer[d.CustomerId], d.CurrentStatus))
                .ToList();
        }

        private static bool IsApproved(string status) => status == StatusApproved || status == StatusDisbursed;

        public async Task<List<CibilRangeRowDTO>> FetchCibilRangeAsync(DateTime? from, DateTime? to)
        {
            var apps = await GetScoredApplicationsAsync(from, to);

            return ScoreBands.Select(b =>
            {
                var sel = apps.Where(a => a.Score >= b.Min && a.Score <= b.Max).ToList();
                var approved = sel.Count(a => IsApproved(a.Status));
                return new CibilRangeRowDTO
                {
                    Range = b.Range,
                    Applicants = sel.Count,
                    Approved = approved,
                    ApprovalPercent = sel.Count > 0 ? Math.Round(approved * 100.0 / sel.Count, 2) : 0
                };
            }).ToList();
        }

        public async Task<List<ScoreCardRowDTO>> FetchScoreCardWiseAsync(DateTime? from, DateTime? to)
        {
            var apps = await GetScoredApplicationsAsync(from, to);

            return ScoreBands.Select(b =>
            {
                var sel = apps.Where(a => a.Score >= b.Min && a.Score <= b.Max).ToList();
                return new ScoreCardRowDTO
                {
                    Status = b.Status,
                    Decision = b.Decision,
                    Applicants = sel.Count,
                    Approved = sel.Count(a => IsApproved(a.Status)),
                    Rejected = sel.Count(a => a.Status == StatusRejected),
                    Pending = sel.Count(a => a.Status == StatusPending)
                };
            }).ToList();
        }

        public async Task<List<FunnelStageDTO>> FetchFunnelAsync(DateTime? from, DateTime? to)
        {
            var (f, t) = Range(from, to);
            var deals = db.LoanDeals.Where(d => d.AppliedDate >= f && d.AppliedDate <= t);

            return new List<FunnelStageDTO>
    {
        new FunnelStageDTO { Stage = "Applications", Count = await deals.CountAsync() },
        new FunnelStageDTO { Stage = "Under Review", Count = await deals.CountAsync(d => d.DealReviews!.Any()) },
        new FunnelStageDTO { Stage = "Approved", Count = await deals.CountAsync(d =>
            d.CurrentStatus == StatusApproved || d.CurrentStatus == StatusDisbursed) },
        new FunnelStageDTO { Stage = "Disbursed", Count = await deals.CountAsync(d =>
            d.CurrentStatus == StatusDisbursed) }
    };
        }

        public async Task<List<CollectionMonthDTO>> FetchCollectionEfficiencyAsync(DateTime? from, DateTime? to)
        {
            var (f, t) = Range(from, to);
            var data = await db.EmiSchedules
                .Where(e => e.DueDate >= f && e.DueDate <= t)
                .GroupBy(e => new { e.DueDate.Year, e.DueDate.Month })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    Billed = g.Sum(x => x.Emi),
                    Collected = g.Where(x => x.PaymentStatus == EmiPaid).Sum(x => x.Emi)
                })
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .ToListAsync();

            return data.Select(x => new CollectionMonthDTO
            {
                Month = x.Year + "-" + x.Month.ToString("00"),
                BilledAmount = x.Billed,
                CollectedAmount = x.Collected,
                EfficiencyPercent = x.Billed > 0 ? Math.Round((double)(x.Collected * 100 / x.Billed), 2) : 0
            }).ToList();
        }
    }
}
