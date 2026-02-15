using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Davivienda.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class AñadirRelacionARE_IDAusuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ARE_ID",
                table: "USUARIO",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_USUARIO_ARE_ID",
                table: "USUARIO",
                column: "ARE_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_USUARIO_AREA_ARE_ID",
                table: "USUARIO",
                column: "ARE_ID",
                principalTable: "AREA",
                principalColumn: "ARE_ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_USUARIO_AREA_ARE_ID",
                table: "USUARIO");

            migrationBuilder.DropIndex(
                name: "IX_USUARIO_ARE_ID",
                table: "USUARIO");

            migrationBuilder.DropColumn(
                name: "ARE_ID",
                table: "USUARIO");
        }
    }
}
