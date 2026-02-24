using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api_security.infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SyncPersistenceModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "content",
                schema: "outbox",
                table: "outboxMessage",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "content",
                schema: "outbox",
                table: "outboxMessage",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
