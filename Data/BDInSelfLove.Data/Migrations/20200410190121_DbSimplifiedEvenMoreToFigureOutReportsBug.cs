using Microsoft.EntityFrameworkCore.Migrations;

namespace BDInSelfLove.Data.Migrations
{
    public partial class DbSimplifiedEvenMoreToFigureOutReportsBug : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Articles_ParentArticleId",
                table: "Comments");

            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Videos_ParentVideoId",
                table: "Comments");

            migrationBuilder.DropIndex(
                name: "IX_Comments_ParentArticleId",
                table: "Comments");

            migrationBuilder.DropIndex(
                name: "IX_Comments_ParentVideoId",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "ParentArticleId",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "ParentVideoId",
                table: "Comments");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ParentArticleId",
                table: "Comments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ParentVideoId",
                table: "Comments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Comments_ParentArticleId",
                table: "Comments",
                column: "ParentArticleId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_ParentVideoId",
                table: "Comments",
                column: "ParentVideoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Articles_ParentArticleId",
                table: "Comments",
                column: "ParentArticleId",
                principalTable: "Articles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Videos_ParentVideoId",
                table: "Comments",
                column: "ParentVideoId",
                principalTable: "Videos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
