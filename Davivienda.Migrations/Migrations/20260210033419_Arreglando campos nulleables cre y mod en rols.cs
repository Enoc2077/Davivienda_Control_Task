using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Davivienda.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class Arreglandocamposnulleablescreymodenrols : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ROL_FEC_MOD",
                table: "ROLES",
                type: "datetimeoffset",
                nullable: false,
                defaultValueSql: "GETDATE()");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ROL_FEC_CRE",
                table: "ROLES",
                type: "datetimeoffset",
                nullable: false,
                defaultValueSql: "GETDATE()");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "ROL_FEC_MOD",
                table: "ROLES",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldDefaultValueSql: "GETDATE()");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "ROL_FEC_CRE",
                table: "ROLES",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldDefaultValueSql: "GETDATE()");
        }
    }
}
