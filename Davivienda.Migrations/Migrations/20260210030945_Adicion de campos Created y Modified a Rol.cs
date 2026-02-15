using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Davivienda.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class AdiciondecamposCreatedyModifiedaRol : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ROL_FEC_CRE",
                table: "ROLES",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ROL_FEC_MOD",
                table: "ROLES",
                type: "datetimeoffset",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ROL_FEC_CRE",
                table: "ROLES");

            migrationBuilder.DropColumn(
                name: "ROL_FEC_MOD",
                table: "ROLES");
        }
    }
}
