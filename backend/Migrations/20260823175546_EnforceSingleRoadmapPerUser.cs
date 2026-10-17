using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SwedenStart.Migrations
{
    /// <inheritdoc />
    public partial class EnforceSingleRoadmapPerUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Roadmaps_UserId",
                table: "Roadmaps");

            migrationBuilder.CreateIndex(
                name: "IX_Roadmaps_UserId",
                table: "Roadmaps",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Roadmaps_UserId",
                table: "Roadmaps");

            migrationBuilder.CreateIndex(
                name: "IX_Roadmaps_UserId",
                table: "Roadmaps",
                column: "UserId");
        }
    }
}
