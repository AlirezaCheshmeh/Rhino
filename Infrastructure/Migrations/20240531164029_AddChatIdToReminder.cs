using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddChatIdToReminder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ChatId",
                schema: "Remind",
                table: "Reminders",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);


            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2024, 5, 31, 20, 10, 28, 989, DateTimeKind.Local).AddTicks(3710));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2024, 5, 31, 20, 10, 28, 989, DateTimeKind.Local).AddTicks(3736));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2024, 5, 31, 20, 10, 28, 989, DateTimeKind.Local).AddTicks(3738));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChatId",
                schema: "Remind",
                table: "Reminders");

            migrationBuilder.DropColumn(
                name: "Type",
                schema: "Remind",
                table: "Reminders");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2024, 5, 20, 21, 17, 48, 716, DateTimeKind.Local).AddTicks(4088));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2024, 5, 20, 21, 17, 48, 716, DateTimeKind.Local).AddTicks(4113));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2024, 5, 20, 21, 17, 48, 716, DateTimeKind.Local).AddTicks(4116));
        }
    }
}
