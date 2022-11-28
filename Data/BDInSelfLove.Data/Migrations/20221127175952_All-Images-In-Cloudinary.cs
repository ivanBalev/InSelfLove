using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BDInSelfLove.Data.Migrations
{
    public partial class AllImagesInCloudinary : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PreviewImageBlob",
                table: "Articles");

            migrationBuilder.AddColumn<string>(
                name: "PreviewImageUrl",
                table: "Articles",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PreviewImageUrl",
                table: "Articles");

            migrationBuilder.AddColumn<byte[]>(
                name: "PreviewImageBlob",
                table: "Articles",
                type: "mediumblob",
                nullable: true);
        }
    }
}
