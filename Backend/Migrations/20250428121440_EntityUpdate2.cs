using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FileStorageProject.Migrations
{
    /// <inheritdoc />
    public partial class EntityUpdate2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "first_name",
                table: "asp_net_users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "last_name",
                table: "asp_net_users",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "first_name",
                table: "asp_net_users");

            migrationBuilder.DropColumn(
                name: "last_name",
                table: "asp_net_users");
        }
    }
}
