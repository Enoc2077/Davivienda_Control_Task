using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Davivienda.Migrations.Migrations
{
    public partial class CambiodetipodedatosdecampoTIE_TOT_TRA : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BIT_SOL_TIE_TOT_TRA",
                table: "BITACORA_SOLUCIONES");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "BIT_SOL_TIE_TOT_TRA",
                table: "BITACORA_SOLUCIONES",
                type: "datetimeoffset",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BIT_SOL_TIE_TOT_TRA",
                table: "BITACORA_SOLUCIONES");

            migrationBuilder.AddColumn<int>(
                name: "BIT_SOL_TIE_TOT_TRA",
                table: "BITACORA_SOLUCIONES",
                type: "int",
                nullable: true);
        }
    }
}