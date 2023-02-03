using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InSelfLove.Data.Migrations
{
    /// <inheritdoc />
    public partial class ImgWidthHeightArticle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ImageHeight",
                table: "Articles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ImageWidth",
                table: "Articles",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageHeight",
                table: "Articles");

            migrationBuilder.DropColumn(
                name: "ImageWidth",
                table: "Articles");
        }
    }
}
