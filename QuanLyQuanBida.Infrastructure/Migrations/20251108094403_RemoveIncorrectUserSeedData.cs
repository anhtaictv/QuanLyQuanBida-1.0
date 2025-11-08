using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyQuanBida.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveIncorrectUserSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "FullName", "IsActive", "PasswordHash", "Phone", "RoleId", "Username" },
                values: new object[] { 1, new DateTime(2025, 11, 8, 9, 39, 49, 330, DateTimeKind.Utc).AddTicks(6095), "Administrator", true, "AQAAAAEAACcQAAAAEKqgkTvtFvY9j8V3Z5t7J5g5X8YfL9g2H9wP7oL6sK9jR8wN5qQ=", "0123456789", 1, "admin" });
        }
    }
}
