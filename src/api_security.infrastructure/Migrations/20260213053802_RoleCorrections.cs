using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api_security.infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RoleCorrections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RoleId",
                table: "UserRole",
                newName: "Role");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Role",
                table: "UserRole",
                newName: "RoleId");
        }
    }
}
