using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIClassification.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialAI : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "classification");

            migrationBuilder.CreateTable(
                name: "InvoiceClassifications",
                schema: "classification",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ExtractedData_InvoiceNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ExtractedData_VendorName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ExtractedData_TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ExtractedData_Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    ExtractedData_IssueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExtractedData_DueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExtractedData_LineItems = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConfidenceScore = table.Column<double>(type: "float", nullable: false),
                    IsDuplicate = table.Column<bool>(type: "bit", nullable: false),
                    IsFraudulent = table.Column<bool>(type: "bit", nullable: false),
                    FraudReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceClassifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OutboxMessages",
                schema: "classification",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OccurredOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProcessedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Error = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxMessages", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_ProcessedOnUtc_OccurredOnUtc",
                schema: "classification",
                table: "OutboxMessages",
                columns: new[] { "ProcessedOnUtc", "OccurredOnUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InvoiceClassifications",
                schema: "classification");

            migrationBuilder.DropTable(
                name: "OutboxMessages",
                schema: "classification");
        }
    }
}
