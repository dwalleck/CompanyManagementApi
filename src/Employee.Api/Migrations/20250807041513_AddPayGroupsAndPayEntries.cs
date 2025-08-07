using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Employee.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddPayGroupsAndPayEntries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "pay_groups",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pay_groups", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "pay_entries",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    pay_group_id = table.Column<Guid>(type: "uuid", nullable: false),
                    employee_id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    account_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    routing_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pay_entries", x => x.id);
                    table.ForeignKey(
                        name: "FK_pay_entries_pay_groups_pay_group_id",
                        column: x => x.pay_group_id,
                        principalTable: "pay_groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_pay_entries_pay_group_id",
                table: "pay_entries",
                column: "pay_group_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "pay_entries");

            migrationBuilder.DropTable(
                name: "pay_groups");
        }
    }
}
