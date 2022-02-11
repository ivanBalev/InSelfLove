using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BDInSelfLove.Data.Migrations
{
    public partial class ArticlePreviewPicAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "PreviewImageBlob",
                table: "Articles",
                type: "blob",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PreviewImageBlob",
                table: "Articles");
        }
    }
}
