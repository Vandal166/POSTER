using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class Changeconversationsentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProfilePicturePath",
                table: "Conversations");

            migrationBuilder.AddColumn<Guid>(
                name: "ProfilePictureID",
                table: "Conversations",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProfilePictureID",
                table: "Conversations");

            migrationBuilder.AddColumn<string>(
                name: "ProfilePicturePath",
                table: "Conversations",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
