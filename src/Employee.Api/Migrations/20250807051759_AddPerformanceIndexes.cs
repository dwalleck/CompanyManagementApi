using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Employee.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_pay_entries_pay_group_id",
                table: "pay_entries",
                newName: "ix_pay_entries_pay_group_id");

            migrationBuilder.RenameIndex(
                name: "IX_pay_entries_disbursement_id",
                table: "pay_entries",
                newName: "ix_pay_entries_disbursement_id");

            migrationBuilder.RenameIndex(
                name: "IX_disbursements_pay_group_id",
                table: "disbursements",
                newName: "ix_disbursements_pay_group_id");

            migrationBuilder.AddColumn<Guid>(
                name: "PayGroupId1",
                table: "pay_entries",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PayGroupId1",
                table: "disbursements",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "created_at",
                table: "disbursements",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "updated_at",
                table: "disbursements",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<Guid>(
                name: "updated_by",
                table: "disbursements",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "ix_pay_entries_employee_id",
                table: "pay_entries",
                column: "employee_id");

            migrationBuilder.CreateIndex(
                name: "IX_pay_entries_PayGroupId1",
                table: "pay_entries",
                column: "PayGroupId1");

            migrationBuilder.CreateIndex(
                name: "ix_pay_entries_type",
                table: "pay_entries",
                column: "type");

            migrationBuilder.CreateIndex(
                name: "ix_disbursements_disbursement_date",
                table: "disbursements",
                column: "disbursement_date");

            migrationBuilder.CreateIndex(
                name: "IX_disbursements_PayGroupId1",
                table: "disbursements",
                column: "PayGroupId1");

            migrationBuilder.CreateIndex(
                name: "ix_disbursements_state",
                table: "disbursements",
                column: "state");

            migrationBuilder.CreateIndex(
                name: "ix_business_employees_email",
                table: "business_employees",
                column: "email",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_disbursements_pay_groups_PayGroupId1",
                table: "disbursements",
                column: "PayGroupId1",
                principalTable: "pay_groups",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_pay_entries_pay_groups_PayGroupId1",
                table: "pay_entries",
                column: "PayGroupId1",
                principalTable: "pay_groups",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_disbursements_pay_groups_PayGroupId1",
                table: "disbursements");

            migrationBuilder.DropForeignKey(
                name: "FK_pay_entries_pay_groups_PayGroupId1",
                table: "pay_entries");

            migrationBuilder.DropIndex(
                name: "ix_pay_entries_employee_id",
                table: "pay_entries");

            migrationBuilder.DropIndex(
                name: "IX_pay_entries_PayGroupId1",
                table: "pay_entries");

            migrationBuilder.DropIndex(
                name: "ix_pay_entries_type",
                table: "pay_entries");

            migrationBuilder.DropIndex(
                name: "ix_disbursements_disbursement_date",
                table: "disbursements");

            migrationBuilder.DropIndex(
                name: "IX_disbursements_PayGroupId1",
                table: "disbursements");

            migrationBuilder.DropIndex(
                name: "ix_disbursements_state",
                table: "disbursements");

            migrationBuilder.DropIndex(
                name: "ix_business_employees_email",
                table: "business_employees");

            migrationBuilder.DropColumn(
                name: "PayGroupId1",
                table: "pay_entries");

            migrationBuilder.DropColumn(
                name: "PayGroupId1",
                table: "disbursements");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "disbursements");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "disbursements");

            migrationBuilder.DropColumn(
                name: "updated_by",
                table: "disbursements");

            migrationBuilder.RenameIndex(
                name: "ix_pay_entries_pay_group_id",
                table: "pay_entries",
                newName: "IX_pay_entries_pay_group_id");

            migrationBuilder.RenameIndex(
                name: "ix_pay_entries_disbursement_id",
                table: "pay_entries",
                newName: "IX_pay_entries_disbursement_id");

            migrationBuilder.RenameIndex(
                name: "ix_disbursements_pay_group_id",
                table: "disbursements",
                newName: "IX_disbursements_pay_group_id");
        }
    }
}
