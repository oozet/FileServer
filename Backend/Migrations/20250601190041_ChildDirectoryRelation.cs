using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FileStorageProject.Migrations
{
    /// <inheritdoc />
    public partial class ChildDirectoryRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "directory_entity_id",
                table: "directories",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_directories_directory_entity_id",
                table: "directories",
                column: "directory_entity_id");

            migrationBuilder.AddForeignKey(
                name: "fk_directories_directories_directory_entity_id",
                table: "directories",
                column: "directory_entity_id",
                principalTable: "directories",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_directories_directories_directory_entity_id",
                table: "directories");

            migrationBuilder.DropIndex(
                name: "ix_directories_directory_entity_id",
                table: "directories");

            migrationBuilder.DropColumn(
                name: "directory_entity_id",
                table: "directories");
        }
    }
}
