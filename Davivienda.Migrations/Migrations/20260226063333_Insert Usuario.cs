using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Davivienda.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class InsertUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AREA",
                columns: new[] { "ARE_ID", "ARE_DES", "ARE_EST", "ARE_FEC_CRE", "ARE_FEC_MOD", "ARE_NOM" },
                values: new object[] { new Guid("f9b2d8e4-7c5a-4b2a-8d3f-1a2b3c4d5e6f"), "Área de gestión global", true, new DateTimeOffset(new DateTime(2026, 2, 26, 0, 33, 32, 225, DateTimeKind.Unspecified).AddTicks(2832), new TimeSpan(0, -6, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 2, 26, 0, 33, 32, 225, DateTimeKind.Unspecified).AddTicks(2861), new TimeSpan(0, -6, 0, 0, 0)), "ADMINISTRACIÓN" });

            migrationBuilder.InsertData(
                table: "ROLES",
                columns: new[] { "ROL_ID", "ROL_DES", "ROL_EST", "ROL_FEC_CRE", "ROL_FEC_MOD", "ROL_NOM" },
                values: new object[] { new Guid("a1b2c3d4-e5f6-4a5b-bc6d-7e8f9a0b1c2d"), "Acceso administrativo total", true, new DateTimeOffset(new DateTime(2026, 2, 26, 0, 33, 32, 225, DateTimeKind.Unspecified).AddTicks(3124), new TimeSpan(0, -6, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 2, 26, 0, 33, 32, 225, DateTimeKind.Unspecified).AddTicks(3127), new TimeSpan(0, -6, 0, 0, 0)), "Gerente" });

            migrationBuilder.InsertData(
                table: "USUARIO",
                columns: new[] { "USU_ID", "ARE_ID", "ROL_ID", "USU_CON", "USU_COR", "USU_EST", "USU_FEC_CRE", "USU_FEC_MOD", "USU_NOM", "USU_NUM", "USU_TEL" },
                values: new object[] { new Guid("d7c8b9a0-1e2f-3a4b-5c6d-7e8f9a0b1c2d"), new Guid("f9b2d8e4-7c5a-4b2a-8d3f-1a2b3c4d5e6f"), new Guid("a1b2c3d4-e5f6-4a5b-bc6d-7e8f9a0b1c2d"), "Admin123", "admin@davivienda.com", true, new DateTimeOffset(new DateTime(2026, 2, 26, 0, 33, 32, 225, DateTimeKind.Unspecified).AddTicks(3174), new TimeSpan(0, -6, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 2, 26, 0, 33, 32, 225, DateTimeKind.Unspecified).AddTicks(3176), new TimeSpan(0, -6, 0, 0, 0)), "admin", "00001", "00000000" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "USUARIO",
                keyColumn: "USU_ID",
                keyValue: new Guid("d7c8b9a0-1e2f-3a4b-5c6d-7e8f9a0b1c2d"));

            migrationBuilder.DeleteData(
                table: "AREA",
                keyColumn: "ARE_ID",
                keyValue: new Guid("f9b2d8e4-7c5a-4b2a-8d3f-1a2b3c4d5e6f"));

            migrationBuilder.DeleteData(
                table: "ROLES",
                keyColumn: "ROL_ID",
                keyValue: new Guid("a1b2c3d4-e5f6-4a5b-bc6d-7e8f9a0b1c2d"));
        }
    }
}
