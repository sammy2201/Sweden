using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SwedenStart.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUserTransportCountyId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TransportCountyId",
                table: "Users");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TransportCountyId",
                table: "Users",
                type: "text",
                nullable: true);
        }
    }
}
