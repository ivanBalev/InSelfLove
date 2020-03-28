using Microsoft.EntityFrameworkCore.Migrations;

namespace BDInSelfLove.Data.Migrations
{
    public partial class CommentsModified : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MainCommentId",
                table: "Comments",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Comments_MainCommentId",
                table: "Comments",
                column: "MainCommentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Comments_MainCommentId",
                table: "Comments",
                column: "MainCommentId",
                principalTable: "Comments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Comments_MainCommentId",
                table: "Comments");

            migrationBuilder.DropIndex(
                name: "IX_Comments_MainCommentId",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "MainCommentId",
                table: "Comments");
        }
    }
}
