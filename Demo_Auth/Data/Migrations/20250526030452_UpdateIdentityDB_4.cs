using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Demo_Auth.Migrations
{
    /// <inheritdoc />
    public partial class UpdateIdentityDB_4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_employees_EmployeeId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_EmployeeId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "EmployeeId",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<string>(
                name: "AppUserId",
                table: "employees",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_employees_AppUserId",
                table: "employees",
                column: "AppUserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_employees_AspNetUsers_AppUserId",
                table: "employees",
                column: "AppUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_employees_AspNetUsers_AppUserId",
                table: "employees");

            migrationBuilder.DropIndex(
                name: "IX_employees_AppUserId",
                table: "employees");

            migrationBuilder.DropColumn(
                name: "AppUserId",
                table: "employees");

            migrationBuilder.AddColumn<int>(
                name: "EmployeeId",
                table: "AspNetUsers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_EmployeeId",
                table: "AspNetUsers",
                column: "EmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_employees_EmployeeId",
                table: "AspNetUsers",
                column: "EmployeeId",
                principalTable: "employees",
                principalColumn: "employee_id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
