using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Davivienda.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class MaxdeDesProcesoyproyecto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PRO_DES",
                table: "PROYECTO",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PROC_DES",
                table: "PROCESO",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "AREA",
                keyColumn: "ARE_ID",
                keyValue: new Guid("f9b2d8e4-7c5a-4b2a-8d3f-1a2b3c4d5e6f"),
                columns: new[] { "ARE_FEC_CRE", "ARE_FEC_MOD" },
                values: new object[] { new DateTimeOffset(new DateTime(2026, 4, 28, 11, 59, 8, 384, DateTimeKind.Unspecified).AddTicks(6469), new TimeSpan(0, -6, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 4, 28, 11, 59, 8, 384, DateTimeKind.Unspecified).AddTicks(6491), new TimeSpan(0, -6, 0, 0, 0)) });

            migrationBuilder.UpdateData(
                table: "ROLES",
                keyColumn: "ROL_ID",
                keyValue: new Guid("a1b2c3d4-e5f6-4a5b-bc6d-7e8f9a0b1c2d"),
                columns: new[] { "ROL_FEC_CRE", "ROL_FEC_MOD" },
                values: new object[] { new DateTimeOffset(new DateTime(2026, 4, 28, 11, 59, 8, 384, DateTimeKind.Unspecified).AddTicks(6665), new TimeSpan(0, -6, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 4, 28, 11, 59, 8, 384, DateTimeKind.Unspecified).AddTicks(6667), new TimeSpan(0, -6, 0, 0, 0)) });

            migrationBuilder.UpdateData(
                table: "USUARIO",
                keyColumn: "USU_ID",
                keyValue: new Guid("d7c8b9a0-1e2f-3a4b-5c6d-7e8f9a0b1c2d"),
                columns: new[] { "USU_FEC_CRE", "USU_FEC_MOD" },
                values: new object[] { new DateTimeOffset(new DateTime(2026, 4, 28, 11, 59, 8, 384, DateTimeKind.Unspecified).AddTicks(6697), new TimeSpan(0, -6, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 4, 28, 11, 59, 8, 384, DateTimeKind.Unspecified).AddTicks(6699), new TimeSpan(0, -6, 0, 0, 0)) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PRO_DES",
                table: "PROYECTO",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PROC_DES",
                table: "PROCESO",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "AREA",
                keyColumn: "ARE_ID",
                keyValue: new Guid("f9b2d8e4-7c5a-4b2a-8d3f-1a2b3c4d5e6f"),
                columns: new[] { "ARE_FEC_CRE", "ARE_FEC_MOD" },
                values: new object[] { new DateTimeOffset(new DateTime(2026, 2, 26, 0, 33, 32, 225, DateTimeKind.Unspecified).AddTicks(2832), new TimeSpan(0, -6, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 2, 26, 0, 33, 32, 225, DateTimeKind.Unspecified).AddTicks(2861), new TimeSpan(0, -6, 0, 0, 0)) });

            migrationBuilder.UpdateData(
                table: "ROLES",
                keyColumn: "ROL_ID",
                keyValue: new Guid("a1b2c3d4-e5f6-4a5b-bc6d-7e8f9a0b1c2d"),
                columns: new[] { "ROL_FEC_CRE", "ROL_FEC_MOD" },
                values: new object[] { new DateTimeOffset(new DateTime(2026, 2, 26, 0, 33, 32, 225, DateTimeKind.Unspecified).AddTicks(3124), new TimeSpan(0, -6, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 2, 26, 0, 33, 32, 225, DateTimeKind.Unspecified).AddTicks(3127), new TimeSpan(0, -6, 0, 0, 0)) });

            migrationBuilder.UpdateData(
                table: "USUARIO",
                keyColumn: "USU_ID",
                keyValue: new Guid("d7c8b9a0-1e2f-3a4b-5c6d-7e8f9a0b1c2d"),
                columns: new[] { "USU_FEC_CRE", "USU_FEC_MOD" },
                values: new object[] { new DateTimeOffset(new DateTime(2026, 2, 26, 0, 33, 32, 225, DateTimeKind.Unspecified).AddTicks(3174), new TimeSpan(0, -6, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 2, 26, 0, 33, 32, 225, DateTimeKind.Unspecified).AddTicks(3176), new TimeSpan(0, -6, 0, 0, 0)) });
        }
    }
}
