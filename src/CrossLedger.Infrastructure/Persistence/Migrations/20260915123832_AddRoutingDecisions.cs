using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrossLedger.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRoutingDecisions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RoutingDecisions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TransferId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProviderCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Rank = table.Column<int>(type: "int", nullable: false),
                    Score = table.Column<decimal>(type: "decimal(9,8)", nullable: false),
                    FeeAmount = table.Column<decimal>(type: "decimal(19,4)", nullable: false),
                    FeeCurrency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    EstimatedSettlementMinutes = table.Column<double>(type: "float", nullable: false),
                    RecordedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoutingDecisions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RoutingDecisions_TransferId",
                table: "RoutingDecisions",
                column: "TransferId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RoutingDecisions");
        }
    }
}
