using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiKnowledgeCopilot.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentProcessingFailureDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FailureReason",
                table: "Documents",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ProcessingCompletedAtUtc",
                table: "Documents",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ProcessingStartedAtUtc",
                table: "Documents",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FailureReason",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "ProcessingCompletedAtUtc",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "ProcessingStartedAtUtc",
                table: "Documents");
        }
    }
}
