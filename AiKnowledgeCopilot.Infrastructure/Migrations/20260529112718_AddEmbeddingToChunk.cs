using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiKnowledgeCopilot.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEmbeddingToChunk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Embedding",
                table: "Chunks",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Embedding",
                table: "Chunks");
        }
    }
}
