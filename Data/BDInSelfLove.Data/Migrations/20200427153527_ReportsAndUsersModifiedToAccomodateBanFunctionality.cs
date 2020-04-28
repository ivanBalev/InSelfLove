using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BDInSelfLove.Data.Migrations
{
    public partial class ReportsAndUsersModifiedToAccomodateBanFunctionality : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OffenderId",
                table: "Reports",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsBanned",
                table: "AspNetUsers",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "Bans",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    ModifiedOn = table.Column<DateTime>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    DeletedOn = table.Column<DateTime>(nullable: true),
                    UserId = table.Column<string>(nullable: true),
                    Reason = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bans_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Reports_OffenderId",
                table: "Reports",
                column: "OffenderId");

            migrationBuilder.CreateIndex(
                name: "IX_Bans_IsDeleted",
                table: "Bans",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Bans_UserId",
                table: "Bans",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_AspNetUsers_OffenderId",
                table: "Reports",
                column: "OffenderId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reports_AspNetUsers_OffenderId",
                table: "Reports");

            migrationBuilder.DropTable(
                name: "Bans");

            migrationBuilder.DropIndex(
                name: "IX_Reports_OffenderId",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "OffenderId",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "IsBanned",
                table: "AspNetUsers");
        }
    }
}
