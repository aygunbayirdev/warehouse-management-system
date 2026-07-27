using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WMS.Modules.Inbound.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "inbound");

            migrationBuilder.CreateTable(
                name: "goods_receipts",
                schema: "inbound",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    warehouse_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    approved_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_goods_receipts", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "goods_receipt_lines",
                schema: "inbound",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    goods_receipt_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_goods_receipt_lines", x => x.id);
                    table.ForeignKey(
                        name: "fk_goods_receipt_lines_goods_receipts_goods_receipt_id",
                        column: x => x.goods_receipt_id,
                        principalSchema: "inbound",
                        principalTable: "goods_receipts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_goods_receipt_lines_goods_receipt_id",
                schema: "inbound",
                table: "goods_receipt_lines",
                column: "goods_receipt_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "goods_receipt_lines",
                schema: "inbound");

            migrationBuilder.DropTable(
                name: "goods_receipts",
                schema: "inbound");
        }
    }
}
