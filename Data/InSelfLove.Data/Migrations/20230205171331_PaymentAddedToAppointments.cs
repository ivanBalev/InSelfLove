using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InSelfLove.Data.Migrations
{
    /// <inheritdoc />
    public partial class PaymentAddedToAppointments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPaid",
                table: "Appointments",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPaid",
                table: "Appointments");
        }
    }
}
