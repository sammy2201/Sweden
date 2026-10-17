using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SwedenStart.Migrations
{
    /// <inheritdoc />
    public partial class RoadmapTaskMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "RoadmapTasks",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Priority",
                table: "RoadmapTasks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ResourceUrl",
                table: "RoadmapTasks",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RuleId",
                table: "RoadmapTasks",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "State",
                table: "RoadmapTasks",
                type: "text",
                nullable: false,
                defaultValue: "Recommended");

            migrationBuilder.AddColumn<string>(
                name: "StateBeforeCompletion",
                table: "RoadmapTasks",
                type: "text",
                nullable: false,
                defaultValue: "Recommended");

            migrationBuilder.Sql("""
                UPDATE "RoadmapTasks"
                SET "Category" = CASE
                        WHEN "Title" ILIKE '%personnummer%' THEN 'Registration'
                        WHEN "Title" ILIKE '%housing%' THEN 'Housing'
                        WHEN "Title" ILIKE '%driving licence%' THEN 'Transport'
                        WHEN "Title" ILIKE '%bankid%' OR "Title" ILIKE '%bank account%' THEN 'Banking'
                        WHEN "Title" ILIKE '%insurance%' OR "Title" ILIKE '%försäkringskassan%' THEN 'Insurance'
                        WHEN "Title" ILIKE '%arbetsförmedlingen%' THEN 'Work'
                        ELSE 'Settlement' END,
                    "Priority" = CASE WHEN "Completed" THEN 1 ELSE 3 END,
                    "State" = CASE WHEN "Completed" THEN 'Completed' ELSE 'Recommended' END,
                    "RuleId" = CASE "Title"
                        WHEN 'Apply for Personnummer' THEN 'personnummer-prepare'
                        WHEN 'Apply for Swedish ID Card' THEN 'id-card'
                        WHEN 'Activate BankID' THEN 'bankid'
                        WHEN 'Register with Försäkringskassan' THEN 'insurance-check'
                        WHEN 'Register with Arbetsförmedlingen' THEN 'work-search'
                        WHEN 'Join Housing Queues' THEN 'housing-search'
                        WHEN 'Check Driving Licence Rules' THEN 'licence-other'
                        ELSE 'legacy-' || "Id"::text END;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "RoadmapTasks");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "RoadmapTasks");

            migrationBuilder.DropColumn(
                name: "ResourceUrl",
                table: "RoadmapTasks");

            migrationBuilder.DropColumn(
                name: "RuleId",
                table: "RoadmapTasks");

            migrationBuilder.DropColumn(
                name: "State",
                table: "RoadmapTasks");

            migrationBuilder.DropColumn(
                name: "StateBeforeCompletion",
                table: "RoadmapTasks");
        }
    }
}
