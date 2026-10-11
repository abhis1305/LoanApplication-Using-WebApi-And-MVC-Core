using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LoanApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddScoreCardLoanOptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "InterestRate",
                table: "ScoreCards",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "TenureInMonths",
                table: "ScoreCards",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InterestRate",
                table: "ScoreCards");

            migrationBuilder.DropColumn(
                name: "TenureInMonths",
                table: "ScoreCards");
        }
    }
}
