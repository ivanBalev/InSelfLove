using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InSelfLove.Data.Migrations
{
    /// <inheritdoc />
    public partial class UserTimezoneIdChangedToTimezone : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "WindowsTimezoneId",
                table: "AspNetUsers",
                newName: "Timezone");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Timezone",
                table: "AspNetUsers",
                newName: "WindowsTimezoneId");
        }
    }
}
