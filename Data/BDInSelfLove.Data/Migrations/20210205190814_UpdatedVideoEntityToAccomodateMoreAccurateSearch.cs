using Microsoft.EntityFrameworkCore.Migrations;

namespace BDInSelfLove.Data.Migrations
{
    public partial class UpdatedVideoEntityToAccomodateMoreAccurateSearch : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AssociatedTerms",
                table: "Videos",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Videos",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AssociatedTerms",
                table: "Videos");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "Videos");
        }
    }
}
