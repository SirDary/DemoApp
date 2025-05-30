using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Demo_Auth.Migrations
{
    /// <inheritdoc />
    public partial class UpdateIdentityDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_employees_EmployeeId",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<int>(
                name: "EmployeeId",
                table: "AspNetUsers",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EployeeId",
                table: "AspNetUsers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_employees_EmployeeId",
                table: "AspNetUsers",
                column: "EmployeeId",
                principalTable: "employees",
                principalColumn: "employee_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_employees_EmployeeId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "EployeeId",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<int>(
                name: "EmployeeId",
                table: "AspNetUsers",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_employees_EmployeeId",
                table: "AspNetUsers",
                column: "EmployeeId",
                principalTable: "employees",
                principalColumn: "employee_id");
        }
    }
}
