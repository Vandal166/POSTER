using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class Changeconversationentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ConversationUser_Conversation_ConversationID",
                table: "ConversationUser");

            migrationBuilder.DropForeignKey(
                name: "FK_ConversationUser_Users_UserID",
                table: "ConversationUser");

            migrationBuilder.DropForeignKey(
                name: "FK_Message_Conversation_ConversationID",
                table: "Message");

            migrationBuilder.DropForeignKey(
                name: "FK_Message_Users_SenderID",
                table: "Message");

            migrationBuilder.DropForeignKey(
                name: "FK_MessageImage_Message_MessageID",
                table: "MessageImage");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MessageImage",
                table: "MessageImage");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Message",
                table: "Message");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ConversationUser",
                table: "ConversationUser");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Conversation",
                table: "Conversation");

            migrationBuilder.RenameTable(
                name: "MessageImage",
                newName: "MessageImages");

            migrationBuilder.RenameTable(
                name: "Message",
                newName: "Messages");

            migrationBuilder.RenameTable(
                name: "ConversationUser",
                newName: "ConversationUsers");

            migrationBuilder.RenameTable(
                name: "Conversation",
                newName: "Conversations");

            migrationBuilder.RenameIndex(
                name: "IX_MessageImage_MessageID",
                table: "MessageImages",
                newName: "IX_MessageImages_MessageID");

            migrationBuilder.RenameIndex(
                name: "IX_Message_SenderID",
                table: "Messages",
                newName: "IX_Messages_SenderID");

            migrationBuilder.RenameIndex(
                name: "IX_Message_ConversationID",
                table: "Messages",
                newName: "IX_Messages_ConversationID");

            migrationBuilder.RenameIndex(
                name: "IX_ConversationUser_UserID",
                table: "ConversationUsers",
                newName: "IX_ConversationUsers_UserID");

            migrationBuilder.AddColumn<Guid>(
                name: "AdminID",
                table: "Conversations",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Conversations",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ProfilePicturePath",
                table: "Conversations",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MessageImages",
                table: "MessageImages",
                column: "ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Messages",
                table: "Messages",
                column: "ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ConversationUsers",
                table: "ConversationUsers",
                columns: new[] { "ConversationID", "UserID" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Conversations",
                table: "Conversations",
                column: "ID");

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

            migrationBuilder.AddForeignKey(
                name: "FK_ConversationUsers_Conversations_ConversationID",
                table: "ConversationUsers",
                column: "ConversationID",
                principalTable: "Conversations",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ConversationUsers_Users_UserID",
                table: "ConversationUsers",
                column: "UserID",
                principalTable: "Users",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MessageImages_Messages_MessageID",
                table: "MessageImages",
                column: "MessageID",
                principalTable: "Messages",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Conversations_ConversationID",
                table: "Messages",
                column: "ConversationID",
                principalTable: "Conversations",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Users_SenderID",
                table: "Messages",
                column: "SenderID",
                principalTable: "Users",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Conversations_Users_AdminID",
                table: "Conversations");

            migrationBuilder.DropForeignKey(
                name: "FK_ConversationUsers_Conversations_ConversationID",
                table: "ConversationUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_ConversationUsers_Users_UserID",
                table: "ConversationUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_MessageImages_Messages_MessageID",
                table: "MessageImages");

            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Conversations_ConversationID",
                table: "Messages");

            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Users_SenderID",
                table: "Messages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Messages",
                table: "Messages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MessageImages",
                table: "MessageImages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ConversationUsers",
                table: "ConversationUsers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Conversations",
                table: "Conversations");

            migrationBuilder.DropIndex(
                name: "IX_Conversations_AdminID",
                table: "Conversations");

            migrationBuilder.DropColumn(
                name: "AdminID",
                table: "Conversations");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Conversations");

            migrationBuilder.DropColumn(
                name: "ProfilePicturePath",
                table: "Conversations");

            migrationBuilder.RenameTable(
                name: "Messages",
                newName: "Message");

            migrationBuilder.RenameTable(
                name: "MessageImages",
                newName: "MessageImage");

            migrationBuilder.RenameTable(
                name: "ConversationUsers",
                newName: "ConversationUser");

            migrationBuilder.RenameTable(
                name: "Conversations",
                newName: "Conversation");

            migrationBuilder.RenameIndex(
                name: "IX_Messages_SenderID",
                table: "Message",
                newName: "IX_Message_SenderID");

            migrationBuilder.RenameIndex(
                name: "IX_Messages_ConversationID",
                table: "Message",
                newName: "IX_Message_ConversationID");

            migrationBuilder.RenameIndex(
                name: "IX_MessageImages_MessageID",
                table: "MessageImage",
                newName: "IX_MessageImage_MessageID");

            migrationBuilder.RenameIndex(
                name: "IX_ConversationUsers_UserID",
                table: "ConversationUser",
                newName: "IX_ConversationUser_UserID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Message",
                table: "Message",
                column: "ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MessageImage",
                table: "MessageImage",
                column: "ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ConversationUser",
                table: "ConversationUser",
                columns: new[] { "ConversationID", "UserID" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Conversation",
                table: "Conversation",
                column: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_ConversationUser_Conversation_ConversationID",
                table: "ConversationUser",
                column: "ConversationID",
                principalTable: "Conversation",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ConversationUser_Users_UserID",
                table: "ConversationUser",
                column: "UserID",
                principalTable: "Users",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Message_Conversation_ConversationID",
                table: "Message",
                column: "ConversationID",
                principalTable: "Conversation",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Message_Users_SenderID",
                table: "Message",
                column: "SenderID",
                principalTable: "Users",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MessageImage_Message_MessageID",
                table: "MessageImage",
                column: "MessageID",
                principalTable: "Message",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
