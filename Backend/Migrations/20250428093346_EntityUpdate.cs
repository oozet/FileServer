using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FileStorageProject.Migrations
{
    /// <inheritdoc />
    public partial class EntityUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "directory",
                table: "files");

            migrationBuilder.DropColumn(
                name: "parent_directory",
                table: "files");

            migrationBuilder.AlterColumn<string>(
                name: "id",
                table: "files",
                type: "text",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<int>(
                name: "directory_id",
                table: "files",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "ix_files_directory_id",
                table: "files",
                column: "directory_id");

            migrationBuilder.AddForeignKey(
                name: "fk_files_directories_directory_id",
                table: "files",
                column: "directory_id",
                principalTable: "directories",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_files_directories_directory_id",
                table: "files");

            migrationBuilder.DropIndex(
                name: "ix_files_directory_id",
                table: "files");

            migrationBuilder.DropColumn(
                name: "directory_id",
                table: "files");

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                table: "files",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "directory",
                table: "files",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "parent_directory",
                table: "files",
                type: "text",
                nullable: true);
        }
    }
}
