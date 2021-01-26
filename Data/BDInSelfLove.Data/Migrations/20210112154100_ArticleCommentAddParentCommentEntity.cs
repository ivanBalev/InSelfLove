using Microsoft.EntityFrameworkCore.Migrations;

namespace BDInSelfLove.Data.Migrations
{
    public partial class ArticleCommentAddParentCommentEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ParentCommentId",
                table: "ArticleComments",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ArticleComments_ParentCommentId",
                table: "ArticleComments",
                column: "ParentCommentId");

            migrationBuilder.AddForeignKey(
                name: "FK_ArticleComments_ArticleComments_ParentCommentId",
                table: "ArticleComments",
                column: "ParentCommentId",
                principalTable: "ArticleComments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ArticleComments_ArticleComments_ParentCommentId",
                table: "ArticleComments");

            migrationBuilder.DropIndex(
                name: "IX_ArticleComments_ParentCommentId",
                table: "ArticleComments");

            migrationBuilder.DropColumn(
                name: "ParentCommentId",
                table: "ArticleComments");
        }
    }
}
