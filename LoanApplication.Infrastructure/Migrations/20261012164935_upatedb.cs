using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LoanApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class upatedb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MonthlyInvestement",
                table: "Customers");

            migrationBuilder.AddColumn<DateTime>(
                name: "WaivedDate",
                table: "PenaltyCharges",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WaiverReason",
                table: "PenaltyCharges",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WaivedDate",
                table: "PenaltyCharges");

            migrationBuilder.DropColumn(
                name: "WaiverReason",
                table: "PenaltyCharges");

            migrationBuilder.AddColumn<decimal>(
                name: "MonthlyInvestement",
                table: "Customers",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
