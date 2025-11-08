using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyQuanBida.Infrastructure.Migrations
{
	public partial class AddMissingEntities : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateTable(
				name: "InventoryTransactions",
				columns: table => new
				{
					Id = table.Column<int>(type: "int", nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					ProductId = table.Column<int>(type: "int", nullable: false),
					Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
					Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
					Reference = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
					CreatedBy = table.Column<int>(type: "int", nullable: false),
					CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_InventoryTransactions", x => x.Id);
					table.ForeignKey(
						name: "FK_InventoryTransactions_Products_ProductId",
						column: x => x.ProductId,
						principalTable: "Products",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
					table.ForeignKey(
						name: "FK_InventoryTransactions_Users_CreatedBy",
						column: x => x.CreatedBy,
						principalTable: "Users",
						principalColumn: "Id",
						onDelete: ReferentialAction.Restrict);
				});

			migrationBuilder.CreateTable(
				name: "Shifts",
				columns: table => new
				{
					Id = table.Column<int>(type: "int", nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					UserId = table.Column<int>(type: "int", nullable: false),
					StartAt = table.Column<DateTime>(type: "datetime2", nullable: false),
					EndAt = table.Column<DateTime>(type: "datetime2", nullable: true),
					OpeningCash = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
					ClosingCash = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
					Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Shifts", x => x.Id);
					table.ForeignKey(
						name: "FK_Shifts_Users_UserId",
						column: x => x.UserId,
						principalTable: "Users",
						principalColumn: "Id",
						onDelete: ReferentialAction.Restrict);
				});

			migrationBuilder.CreateTable(
				name: "AuditLogs",
				columns: table => new
				{
					Id = table.Column<int>(type: "int", nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					UserId = table.Column<int>(type: "int", nullable: false),
					Action = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
					TargetTable = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
					TargetId = table.Column<int>(type: "int", nullable: true),
					OldValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
					NewValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
					CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_AuditLogs", x => x.Id);
					table.ForeignKey(
						name: "FK_AuditLogs_Users_UserId",
						column: x => x.UserId,
						principalTable: "Users",
						principalColumn: "Id",
						onDelete: ReferentialAction.Restrict);
				});

			migrationBuilder.CreateIndex(
				name: "IX_InventoryTransactions_CreatedBy",
				table: "InventoryTransactions",
				column: "CreatedBy");

			migrationBuilder.CreateIndex(
				name: "IX_InventoryTransactions_ProductId",
				table: "InventoryTransactions",
				column: "ProductId");

			migrationBuilder.CreateIndex(
				name: "IX_Shifts_UserId",
				table: "Shifts",
				column: "UserId");

			migrationBuilder.CreateIndex(
				name: "IX_AuditLogs_UserId",
				table: "AuditLogs",
				column: "UserId");
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "InventoryTransactions");

			migrationBuilder.DropTable(
				name: "Shifts");

			migrationBuilder.DropTable(
				name: "AuditLogs");
		}
	}
}