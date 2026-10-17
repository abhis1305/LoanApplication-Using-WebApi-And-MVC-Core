using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LoanApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class loan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    CustomerId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Age = table.Column<int>(type: "int", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MobileNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Pan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AadhaarNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmploymentType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MonthlyIncome = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MonthlyInvestment = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    IsEmailVerified = table.Column<bool>(type: "bit", nullable: false),
                    IsTwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorSecret = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.CustomerId);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    NotificationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.NotificationId);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    RoleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.RoleId);
                });

            migrationBuilder.CreateTable(
                name: "CibilReports",
                columns: table => new
                {
                    CibilReportId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    PanNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CibilScore = table.Column<int>(type: "int", nullable: false),
                    CheckDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CibilReports", x => x.CibilReportId);
                    table.ForeignKey(
                        name: "FK_CibilReports_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "CustomerId");
                });

            migrationBuilder.CreateTable(
                name: "EligibilityResults",
                columns: table => new
                {
                    EligibilityId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    CibilScore = table.Column<int>(type: "int", nullable: false),
                    IsEligible = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LoanAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RejectionReason = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EligibilityResults", x => x.EligibilityId);
                    table.ForeignKey(
                        name: "FK_EligibilityResults_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "CustomerId");
                });

            migrationBuilder.CreateTable(
                name: "KycDocuments",
                columns: table => new
                {
                    DocumentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    DocumentType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VerificationStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RejectionReason = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KycDocuments", x => x.DocumentId);
                    table.ForeignKey(
                        name: "FK_KycDocuments_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "CustomerId");
                });

            migrationBuilder.CreateTable(
                name: "LoanDeals",
                columns: table => new
                {
                    DealId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    LoanType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LoanAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InterestRate = table.Column<double>(type: "float", nullable: false),
                    TenureMonths = table.Column<int>(type: "int", nullable: false),
                    EmiAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BankName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BankAccountNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IFSCCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EmiDay = table.Column<int>(type: "int", nullable: false),
                    ApprovedAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CurrentStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RejectionReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AppliedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoanDeals", x => x.DealId);
                    table.ForeignKey(
                        name: "FK_LoanDeals_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "CustomerId");
                });

            migrationBuilder.CreateTable(
                name: "ScoreCards",
                columns: table => new
                {
                    ScoreCardId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    RiskCategory = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InterestRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TenureInMonths = table.Column<int>(type: "int", nullable: false),
                    EligibleLoanAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScoreCards", x => x.ScoreCardId);
                    table.ForeignKey(
                        name: "FK_ScoreCards_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "CustomerId");
                });

            migrationBuilder.CreateTable(
                name: "Wallets",
                columns: table => new
                {
                    WalletId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    Balance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wallets", x => x.WalletId);
                    table.ForeignKey(
                        name: "FK_Wallets_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "CustomerId");
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: true),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Mobile = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsTwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorSecret = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_Users_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "CustomerId");
                    table.ForeignKey(
                        name: "FK_Users_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "RoleId");
                });

            migrationBuilder.CreateTable(
                name: "Disbursements",
                columns: table => new
                {
                    DisbursementId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DealId = table.Column<int>(type: "int", nullable: false),
                    DisburseAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BankPartner = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DisbursementDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Disbursements", x => x.DisbursementId);
                    table.ForeignKey(
                        name: "FK_Disbursements_LoanDeals_DealId",
                        column: x => x.DealId,
                        principalTable: "LoanDeals",
                        principalColumn: "DealId");
                });

            migrationBuilder.CreateTable(
                name: "LoanAccounts",
                columns: table => new
                {
                    LoanAccountId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DealId = table.Column<int>(type: "int", nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    LoanAccountNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LoanAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OutstandingPrincipal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LoanStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InterestRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TenureMonths = table.Column<int>(type: "int", nullable: false),
                    EmiAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DisbursementDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalPaidAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoanAccounts", x => x.LoanAccountId);
                    table.ForeignKey(
                        name: "FK_LoanAccounts_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "CustomerId");
                    table.ForeignKey(
                        name: "FK_LoanAccounts_LoanDeals_DealId",
                        column: x => x.DealId,
                        principalTable: "LoanDeals",
                        principalColumn: "DealId");
                });

            migrationBuilder.CreateTable(
                name: "SanctionLetters",
                columns: table => new
                {
                    SanctionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DealId = table.Column<int>(type: "int", nullable: false),
                    LoanAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InterestRate = table.Column<double>(type: "float", nullable: false),
                    TenureMonths = table.Column<int>(type: "int", nullable: false),
                    EmiAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SanctionLetters", x => x.SanctionId);
                    table.ForeignKey(
                        name: "FK_SanctionLetters_LoanDeals_DealId",
                        column: x => x.DealId,
                        principalTable: "LoanDeals",
                        principalColumn: "DealId");
                });

            migrationBuilder.CreateTable(
                name: "WalletTransactions",
                columns: table => new
                {
                    TransactionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WalletId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TransactionType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReferenceId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TransactionDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WalletTransactions", x => x.TransactionId);
                    table.ForeignKey(
                        name: "FK_WalletTransactions_Wallets_WalletId",
                        column: x => x.WalletId,
                        principalTable: "Wallets",
                        principalColumn: "WalletId");
                });

            migrationBuilder.CreateTable(
                name: "DealReviews",
                columns: table => new
                {
                    ReviewId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DealId = table.Column<int>(type: "int", nullable: false),
                    OfficerId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DealReviews", x => x.ReviewId);
                    table.ForeignKey(
                        name: "FK_DealReviews_LoanDeals_DealId",
                        column: x => x.DealId,
                        principalTable: "LoanDeals",
                        principalColumn: "DealId");
                    table.ForeignKey(
                        name: "FK_DealReviews_Users_OfficerId",
                        column: x => x.OfficerId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "EmiSchedules",
                columns: table => new
                {
                    EmiScheduleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LoanAccountId = table.Column<int>(type: "int", nullable: false),
                    InstallmentNo = table.Column<int>(type: "int", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Emi = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PrincipalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InterestAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OpeningBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ClosingBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaymentStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PaidDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmiSchedules", x => x.EmiScheduleId);
                    table.ForeignKey(
                        name: "FK_EmiSchedules_LoanAccounts_LoanAccountId",
                        column: x => x.LoanAccountId,
                        principalTable: "LoanAccounts",
                        principalColumn: "LoanAccountId");
                });

            migrationBuilder.CreateTable(
                name: "ForeClosureRequests",
                columns: table => new
                {
                    RequestId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LoanAccountId = table.Column<int>(type: "int", nullable: false),
                    ForeClosureType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ForeClosureAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PartialAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RequestedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpectedClosureDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ApprovedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsPaid = table.Column<bool>(type: "bit", nullable: false),
                    PaidDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClosedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ForeClosureRequests", x => x.RequestId);
                    table.ForeignKey(
                        name: "FK_ForeClosureRequests_LoanAccounts_LoanAccountId",
                        column: x => x.LoanAccountId,
                        principalTable: "LoanAccounts",
                        principalColumn: "LoanAccountId");
                });

            migrationBuilder.CreateTable(
                name: "InterestAccruals",
                columns: table => new
                {
                    AccrualId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LoanAccountId = table.Column<int>(type: "int", nullable: false),
                    InterestAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AccrualDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    InstallmentNo = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InterestAccruals", x => x.AccrualId);
                    table.ForeignKey(
                        name: "FK_InterestAccruals_LoanAccounts_LoanAccountId",
                        column: x => x.LoanAccountId,
                        principalTable: "LoanAccounts",
                        principalColumn: "LoanAccountId");
                });

            migrationBuilder.CreateTable(
                name: "LoanClosures",
                columns: table => new
                {
                    ClosureId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LoanAccountId = table.Column<int>(type: "int", nullable: false),
                    ClosureType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FinalSettlementAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ClosureDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ClosedBy = table.Column<int>(type: "int", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClosureStatus = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoanClosures", x => x.ClosureId);
                    table.ForeignKey(
                        name: "FK_LoanClosures_LoanAccounts_LoanAccountId",
                        column: x => x.LoanAccountId,
                        principalTable: "LoanAccounts",
                        principalColumn: "LoanAccountId");
                    table.ForeignKey(
                        name: "FK_LoanClosures_Users_ClosedBy",
                        column: x => x.ClosedBy,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "LoanPayments",
                columns: table => new
                {
                    PaymentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LoanAccountId = table.Column<int>(type: "int", nullable: false),
                    PaymentName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PaidAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaymentMode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TransactionReference = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PrincipalPaid = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InterestPaid = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PenaltyPaid = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaymentStatus = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoanPayments", x => x.PaymentId);
                    table.ForeignKey(
                        name: "FK_LoanPayments_LoanAccounts_LoanAccountId",
                        column: x => x.LoanAccountId,
                        principalTable: "LoanAccounts",
                        principalColumn: "LoanAccountId");
                });

            migrationBuilder.CreateTable(
                name: "SupportTickets",
                columns: table => new
                {
                    TicketId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    LoanAccountId = table.Column<int>(type: "int", nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AttachmentPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OfficerResponse = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupportTickets", x => x.TicketId);
                    table.ForeignKey(
                        name: "FK_SupportTickets_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "CustomerId");
                    table.ForeignKey(
                        name: "FK_SupportTickets_LoanAccounts_LoanAccountId",
                        column: x => x.LoanAccountId,
                        principalTable: "LoanAccounts",
                        principalColumn: "LoanAccountId");
                });

            migrationBuilder.CreateTable(
                name: "PenaltyCharges",
                columns: table => new
                {
                    PenaltyId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LoanAccountId = table.Column<int>(type: "int", nullable: false),
                    EmiScheduleId = table.Column<int>(type: "int", nullable: false),
                    PenaltyType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PenaltyAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DelayDays = table.Column<int>(type: "int", nullable: false),
                    AppliedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WaiverReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WaivedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PenaltyCharges", x => x.PenaltyId);
                    table.ForeignKey(
                        name: "FK_PenaltyCharges_EmiSchedules_EmiScheduleId",
                        column: x => x.EmiScheduleId,
                        principalTable: "EmiSchedules",
                        principalColumn: "EmiScheduleId");
                    table.ForeignKey(
                        name: "FK_PenaltyCharges_LoanAccounts_LoanAccountId",
                        column: x => x.LoanAccountId,
                        principalTable: "LoanAccounts",
                        principalColumn: "LoanAccountId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CibilReports_CustomerId",
                table: "CibilReports",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_DealReviews_DealId",
                table: "DealReviews",
                column: "DealId");

            migrationBuilder.CreateIndex(
                name: "IX_DealReviews_OfficerId",
                table: "DealReviews",
                column: "OfficerId");

            migrationBuilder.CreateIndex(
                name: "IX_Disbursements_DealId",
                table: "Disbursements",
                column: "DealId");

            migrationBuilder.CreateIndex(
                name: "IX_EligibilityResults_CustomerId",
                table: "EligibilityResults",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_EmiSchedules_LoanAccountId",
                table: "EmiSchedules",
                column: "LoanAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_ForeClosureRequests_LoanAccountId",
                table: "ForeClosureRequests",
                column: "LoanAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_InterestAccruals_LoanAccountId",
                table: "InterestAccruals",
                column: "LoanAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_KycDocuments_CustomerId",
                table: "KycDocuments",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_LoanAccounts_CustomerId",
                table: "LoanAccounts",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_LoanAccounts_DealId",
                table: "LoanAccounts",
                column: "DealId");

            migrationBuilder.CreateIndex(
                name: "IX_LoanClosures_ClosedBy",
                table: "LoanClosures",
                column: "ClosedBy");

            migrationBuilder.CreateIndex(
                name: "IX_LoanClosures_LoanAccountId",
                table: "LoanClosures",
                column: "LoanAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_LoanDeals_CustomerId",
                table: "LoanDeals",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_LoanPayments_LoanAccountId",
                table: "LoanPayments",
                column: "LoanAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_PenaltyCharges_EmiScheduleId",
                table: "PenaltyCharges",
                column: "EmiScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_PenaltyCharges_LoanAccountId",
                table: "PenaltyCharges",
                column: "LoanAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_SanctionLetters_DealId",
                table: "SanctionLetters",
                column: "DealId");

            migrationBuilder.CreateIndex(
                name: "IX_ScoreCards_CustomerId",
                table: "ScoreCards",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_SupportTickets_CustomerId",
                table: "SupportTickets",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_SupportTickets_LoanAccountId",
                table: "SupportTickets",
                column: "LoanAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_CustomerId",
                table: "Users",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_RoleId",
                table: "Users",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Wallets_CustomerId",
                table: "Wallets",
                column: "CustomerId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WalletTransactions_WalletId",
                table: "WalletTransactions",
                column: "WalletId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CibilReports");

            migrationBuilder.DropTable(
                name: "DealReviews");

            migrationBuilder.DropTable(
                name: "Disbursements");

            migrationBuilder.DropTable(
                name: "EligibilityResults");

            migrationBuilder.DropTable(
                name: "ForeClosureRequests");

            migrationBuilder.DropTable(
                name: "InterestAccruals");

            migrationBuilder.DropTable(
                name: "KycDocuments");

            migrationBuilder.DropTable(
                name: "LoanClosures");

            migrationBuilder.DropTable(
                name: "LoanPayments");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "PenaltyCharges");

            migrationBuilder.DropTable(
                name: "SanctionLetters");

            migrationBuilder.DropTable(
                name: "ScoreCards");

            migrationBuilder.DropTable(
                name: "SupportTickets");

            migrationBuilder.DropTable(
                name: "WalletTransactions");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "EmiSchedules");

            migrationBuilder.DropTable(
                name: "Wallets");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "LoanAccounts");

            migrationBuilder.DropTable(
                name: "LoanDeals");

            migrationBuilder.DropTable(
                name: "Customers");
        }
    }
}
