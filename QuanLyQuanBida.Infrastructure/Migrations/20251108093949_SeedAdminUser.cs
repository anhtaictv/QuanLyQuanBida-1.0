using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyQuanBida.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedAdminUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "FullName", "IsActive", "PasswordHash", "Phone", "RoleId", "Username" },
                values: new object[] { 1, new DateTime(2025, 11, 8, 9, 39, 49, 330, DateTimeKind.Utc).AddTicks(6095), "Administrator", true, "$2y$12$RQm3JerPsue4jHpbFe2mKuPQuvhPQbY4/8ZZaVC3nNiPqtIoZ3R06", "0123456789", 1, "admin" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1);
        }
    }
}
