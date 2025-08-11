using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class ChangeConversationentity2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Conversations_Users_AdminID",
                table: "Conversations");

            migrationBuilder.DropIndex(
                name: "IX_Conversations_AdminID",
                table: "Conversations");

            migrationBuilder.DropColumn(
                name: "AdminID",
                table: "Conversations");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByID",
                table: "Conversations",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_CreatedByID",
                table: "Conversations",
                column: "CreatedByID");

            migrationBuilder.AddForeignKey(
                name: "FK_Conversations_Users_CreatedByID",
                table: "Conversations",
                column: "CreatedByID",
                principalTable: "Users",
                principalColumn: "ID",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Conversations_Users_CreatedByID",
                table: "Conversations");

            migrationBuilder.DropIndex(
                name: "IX_Conversations_CreatedByID",
                table: "Conversations");

            migrationBuilder.DropColumn(
                name: "CreatedByID",
                table: "Conversations");

            migrationBuilder.AddColumn<Guid>(
                name: "AdminID",
                table: "Conversations",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_AdminID",
                table: "Conversations",
                column: "AdminID");

            migrationBuilder.AddForeignKey(
                name: "FK_Conversations_Users_AdminID",
                table: "Conversations",
                column: "AdminID",
                principalTable: "Users",
                principalColumn: "ID",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
