using System;
using System.Collections.Generic;
using Employee.Api.Types;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Employee.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddBusinessEmployeeWithJsonBankAccounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "business_employees",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    bank_accounts = table.Column<List<BankAccount>>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_business_employees", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "business_employees");
        }
    }
}
