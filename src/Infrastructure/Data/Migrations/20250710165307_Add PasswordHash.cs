using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPasswordHash : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserID1",
                table: "Posts",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Posts_UserID1",
                table: "Posts",
                column: "UserID1");

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_Users_UserID1",
                table: "Posts",
                column: "UserID1",
                principalTable: "Users",
                principalColumn: "ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Posts_Users_UserID1",
                table: "Posts");

            migrationBuilder.DropIndex(
                name: "IX_Posts_UserID1",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UserID1",
                table: "Posts");
        }
    }
}
