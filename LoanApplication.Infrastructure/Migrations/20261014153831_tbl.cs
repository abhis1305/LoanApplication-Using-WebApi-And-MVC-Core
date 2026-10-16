using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LoanApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class tbl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "KycDocuments",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "KycDocuments");
        }
    }
}
