using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SwedenStart.Migrations
{
    /// <inheritdoc />
    public partial class RemoveRoadmapTaskPrerequisites : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrerequisitesJson",
                table: "RoadmapTasks");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PrerequisitesJson",
                table: "RoadmapTasks",
                type: "text",
                nullable: false,
                defaultValue: "[]");
        }
    }
}
