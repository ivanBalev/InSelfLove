using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BDInSelfLove.Data.Migrations
{
    public partial class ArticlePreviewBlobSizeIncrease : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<byte[]>(
                name: "PreviewImageBlob",
                table: "Articles",
                type: "mediumblob",
                nullable: true,
                oldClrType: typeof(byte[]),
                oldType: "blob",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<byte[]>(
                name: "PreviewImageBlob",
                table: "Articles",
                type: "blob",
                nullable: true,
                oldClrType: typeof(byte[]),
                oldType: "mediumblob",
                oldNullable: true);
        }
    }
}
