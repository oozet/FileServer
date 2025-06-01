using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FileStorageProject.Migrations
{
    /// <inheritdoc />
    public partial class FixChildDirRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.CreateIndex(
                name: "ix_directories_parent_directory_id",
                table: "directories",
                column: "parent_directory_id");

            migrationBuilder.AddForeignKey(
                name: "fk_directories_directories_parent_directory_id",
                table: "directories",
                column: "parent_directory_id",
                principalTable: "directories",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_directories_directories_parent_directory_id",
                table: "directories");

            migrationBuilder.DropIndex(
                name: "ix_directories_parent_directory_id",
                table: "directories");

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
    }
}
