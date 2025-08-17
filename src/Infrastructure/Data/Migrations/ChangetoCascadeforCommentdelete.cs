using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class ChangetoCascadeforCommentdelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER TABLE ""Comments"" 
                DROP CONSTRAINT ""FK_Comments_Comments_ParentCommentID"";

                ALTER TABLE ""Comments""
                ADD CONSTRAINT ""FK_Comments_Comments_ParentCommentID""
                    FOREIGN KEY (""ParentCommentID"") 
                    REFERENCES ""Comments"" (""ID"")
                    ON DELETE CASCADE;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
