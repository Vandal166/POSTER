using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class Fixdb_ctx : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Posts_Users_UserID1",
                table: "Posts");

            migrationBuilder.DropIndex(
                name: "IX_Posts_UserID1",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "UserID1",
                table: "Posts");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
    }
}
