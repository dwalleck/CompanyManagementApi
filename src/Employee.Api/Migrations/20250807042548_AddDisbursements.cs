using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Employee.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddDisbursements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string[]>(
                name: "approvers",
                table: "pay_groups",
                type: "text[]",
                nullable: false,
                defaultValue: new string[0]);

            migrationBuilder.CreateTable(
                name: "disbursements",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    pay_group_id = table.Column<Guid>(type: "uuid", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    disbursement_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    transaction_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_disbursements", x => x.id);
                    table.ForeignKey(
                        name: "FK_disbursements_pay_groups_pay_group_id",
                        column: x => x.pay_group_id,
                        principalTable: "pay_groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_disbursements_pay_group_id",
                table: "disbursements",
                column: "pay_group_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "disbursements");

            migrationBuilder.DropColumn(
                name: "approvers",
                table: "pay_groups");
        }
    }
}
