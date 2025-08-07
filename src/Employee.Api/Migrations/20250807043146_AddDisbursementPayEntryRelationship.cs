using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Employee.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddDisbursementPayEntryRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "amount",
                table: "disbursements");

            migrationBuilder.DropColumn(
                name: "status",
                table: "disbursements");

            migrationBuilder.DropColumn(
                name: "transaction_id",
                table: "disbursements");

            migrationBuilder.AddColumn<Guid>(
                name: "disbursement_id",
                table: "pay_entries",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "state",
                table: "disbursements",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_pay_entries_disbursement_id",
                table: "pay_entries",
                column: "disbursement_id");

            migrationBuilder.AddForeignKey(
                name: "FK_pay_entries_disbursements_disbursement_id",
                table: "pay_entries",
                column: "disbursement_id",
                principalTable: "disbursements",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_pay_entries_disbursements_disbursement_id",
                table: "pay_entries");

            migrationBuilder.DropIndex(
                name: "IX_pay_entries_disbursement_id",
                table: "pay_entries");

            migrationBuilder.DropColumn(
                name: "disbursement_id",
                table: "pay_entries");

            migrationBuilder.DropColumn(
                name: "state",
                table: "disbursements");

            migrationBuilder.AddColumn<decimal>(
                name: "amount",
                table: "disbursements",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "status",
                table: "disbursements",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "transaction_id",
                table: "disbursements",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }
    }
}
