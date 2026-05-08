using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnterprisePortal.Migrations
{
    /// <inheritdoc />
    public partial class AddStartEndHour : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EndHour",
                table: "OvertimeApplications",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StartHour",
                table: "OvertimeApplications",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EndHour",
                table: "LeaveApplications",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StartHour",
                table: "LeaveApplications",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndHour",
                table: "OvertimeApplications");

            migrationBuilder.DropColumn(
                name: "StartHour",
                table: "OvertimeApplications");

            migrationBuilder.DropColumn(
                name: "EndHour",
                table: "LeaveApplications");

            migrationBuilder.DropColumn(
                name: "StartHour",
                table: "LeaveApplications");
        }
    }
}
