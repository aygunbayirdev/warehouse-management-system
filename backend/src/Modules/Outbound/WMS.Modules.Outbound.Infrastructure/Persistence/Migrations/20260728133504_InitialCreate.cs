using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WMS.Modules.Outbound.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "outbound");

            migrationBuilder.CreateTable(
                name: "goods_issues",
                schema: "outbound",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    warehouse_id = table.Column<Guid>(type: "uuid", nullable: false),
                    destination = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    approved_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_goods_issues", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "goods_issue_lines",
                schema: "outbound",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    goods_issue_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_goods_issue_lines", x => x.id);
                    table.ForeignKey(
                        name: "fk_goods_issue_lines_goods_issues_goods_issue_id",
                        column: x => x.goods_issue_id,
                        principalSchema: "outbound",
                        principalTable: "goods_issues",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_goods_issue_lines_goods_issue_id",
                schema: "outbound",
                table: "goods_issue_lines",
                column: "goods_issue_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "goods_issue_lines",
                schema: "outbound");

            migrationBuilder.DropTable(
                name: "goods_issues",
                schema: "outbound");
        }
    }
}
