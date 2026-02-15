using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Davivienda.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class MigracionInicialCompleta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AREA",
                columns: table => new
                {
                    ARE_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ARE_NOM = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ARE_DES = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ARE_EST = table.Column<bool>(type: "bit", nullable: false),
                    ARE_FEC_CRE = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AREA", x => x.ARE_ID);
                });

            migrationBuilder.CreateTable(
                name: "PRIORIDAD",
                columns: table => new
                {
                    PRI_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PRI_NOM = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PRI_DES = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PRI_NIV = table.Column<int>(type: "int", nullable: true),
                    PRI_FEC_CRE = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    PRI_FEC_MOD = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PRIORIDAD", x => x.PRI_ID);
                });

            migrationBuilder.CreateTable(
                name: "ROLES",
                columns: table => new
                {
                    ROL_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ROL_NOM = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ROL_DES = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ROL_EST = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ROLES", x => x.ROL_ID);
                });

            migrationBuilder.CreateTable(
                name: "PROYECTO",
                columns: table => new
                {
                    PRO_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PRO_NOM = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PRO_DES = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    PRO_FEC_INI = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    PRO_FEC_FIN = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    PRO_EST = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ARE_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PRO_FEC_CRE = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    PRO_FEC_MOD = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PROYECTO", x => x.PRO_ID);
                    table.ForeignKey(
                        name: "FK_PROYECTO_AREA_ARE_ID",
                        column: x => x.ARE_ID,
                        principalTable: "AREA",
                        principalColumn: "ARE_ID");
                });

            migrationBuilder.CreateTable(
                name: "USUARIO",
                columns: table => new
                {
                    USU_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    USU_NOM = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    USU_NUM = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    USU_COR = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    USU_CON = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    USU_TEL = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: true),
                    USU_EST = table.Column<bool>(type: "bit", nullable: true),
                    ROL_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    USU_FEC_CRE = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    USU_FEC_MOD = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_USUARIO", x => x.USU_ID);
                    table.ForeignKey(
                        name: "FK_USUARIO_ROLES_ROL_ID",
                        column: x => x.ROL_ID,
                        principalTable: "ROLES",
                        principalColumn: "ROL_ID");
                });

            migrationBuilder.CreateTable(
                name: "PROCESO",
                columns: table => new
                {
                    PROC_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PROC_NOM = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PROC_DES = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    PROC_FRE = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PROC_EST = table.Column<bool>(type: "bit", nullable: true),
                    PROC_FEC_CRE = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    PROC_FEC_MOD = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    PRO_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PROCESO", x => x.PROC_ID);
                    table.ForeignKey(
                        name: "FK_PROCESO_PROYECTO_PRO_ID",
                        column: x => x.PRO_ID,
                        principalTable: "PROYECTO",
                        principalColumn: "PRO_ID");
                });

            migrationBuilder.CreateTable(
                name: "DETALLES_PROYECTO",
                columns: table => new
                {
                    DET_PRO_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PRO_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    USU_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ROL_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DET_PRO_FEC_ASI = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DET_PRO_FEC_CRE = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DET_PRO_FEC_MOD = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DETALLES_PROYECTO", x => x.DET_PRO_ID);
                    table.ForeignKey(
                        name: "FK_DETALLES_PROYECTO_PROYECTO_PRO_ID",
                        column: x => x.PRO_ID,
                        principalTable: "PROYECTO",
                        principalColumn: "PRO_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DETALLES_PROYECTO_ROLES_ROL_ID",
                        column: x => x.ROL_ID,
                        principalTable: "ROLES",
                        principalColumn: "ROL_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DETALLES_PROYECTO_USUARIO_USU_ID",
                        column: x => x.USU_ID,
                        principalTable: "USUARIO",
                        principalColumn: "USU_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NOTIFICACIONES",
                columns: table => new
                {
                    NOT_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NOT_MEN = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    NOT_TIP = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    NOT_LEI = table.Column<bool>(type: "bit", nullable: false),
                    NOT_FEC_CRE = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    NOT_FEC_MOD = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    USU_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NOTIFICACIONES", x => x.NOT_ID);
                    table.ForeignKey(
                        name: "FK_NOTIFICACIONES_USUARIO_USU_ID",
                        column: x => x.USU_ID,
                        principalTable: "USUARIO",
                        principalColumn: "USU_ID");
                });

            migrationBuilder.CreateTable(
                name: "TAREA",
                columns: table => new
                {
                    TAR_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TAR_NOM = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TAR_DES = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TAR_EST = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TAR_FEC_INI = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    TAR_FEC_FIN = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    TAR_FEC_CRE = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    TAR_FEC_MOD = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    PROC_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    USU_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PRI_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TAREA", x => x.TAR_ID);
                    table.ForeignKey(
                        name: "FK_TAREA_PRIORIDAD_PRI_ID",
                        column: x => x.PRI_ID,
                        principalTable: "PRIORIDAD",
                        principalColumn: "PRI_ID");
                    table.ForeignKey(
                        name: "FK_TAREA_PROCESO_PROC_ID",
                        column: x => x.PROC_ID,
                        principalTable: "PROCESO",
                        principalColumn: "PROC_ID");
                    table.ForeignKey(
                        name: "FK_TAREA_USUARIO_USU_ID",
                        column: x => x.USU_ID,
                        principalTable: "USUARIO",
                        principalColumn: "USU_ID");
                });

            migrationBuilder.CreateTable(
                name: "DOCUMENTACION",
                columns: table => new
                {
                    DOC_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DOC_NOM = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DOC_RUT = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    DOC_FEC_CRE = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DOC_FEC_MOD = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    TAR_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    USU_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DOCUMENTACION", x => x.DOC_ID);
                    table.ForeignKey(
                        name: "FK_DOCUMENTACION_TAREA_TAR_ID",
                        column: x => x.TAR_ID,
                        principalTable: "TAREA",
                        principalColumn: "TAR_ID");
                    table.ForeignKey(
                        name: "FK_DOCUMENTACION_USUARIO_USU_ID",
                        column: x => x.USU_ID,
                        principalTable: "USUARIO",
                        principalColumn: "USU_ID");
                });

            migrationBuilder.CreateTable(
                name: "FRICCION",
                columns: table => new
                {
                    FRI_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FRI_TIP = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FRI_DES = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    FRI_EST = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FRI_IMP = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FRI_FEC_CRE = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    FRI_FEC_MOD = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    TAR_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    USU_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FRICCION", x => x.FRI_ID);
                    table.ForeignKey(
                        name: "FK_FRICCION_TAREA_TAR_ID",
                        column: x => x.TAR_ID,
                        principalTable: "TAREA",
                        principalColumn: "TAR_ID");
                    table.ForeignKey(
                        name: "FK_FRICCION_USUARIO_USU_ID",
                        column: x => x.USU_ID,
                        principalTable: "USUARIO",
                        principalColumn: "USU_ID");
                });

            migrationBuilder.CreateTable(
                name: "BITACORA_FRICCIONES",
                columns: table => new
                {
                    BIT_FRI_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BIT_FRI_NOM = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    BIT_FRI_DES = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    BIT_FRI_EST = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BIT_FRI_FEC_CRE = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    BIT_FRI_FEC_MOD = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    USU_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FRI_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BITACORA_FRICCIONES", x => x.BIT_FRI_ID);
                    table.ForeignKey(
                        name: "FK_BITACORA_FRICCIONES_FRICCION_FRI_ID",
                        column: x => x.FRI_ID,
                        principalTable: "FRICCION",
                        principalColumn: "FRI_ID");
                    table.ForeignKey(
                        name: "FK_BITACORA_FRICCIONES_USUARIO_USU_ID",
                        column: x => x.USU_ID,
                        principalTable: "USUARIO",
                        principalColumn: "USU_ID");
                });

            migrationBuilder.CreateTable(
                name: "COMENTARIOS",
                columns: table => new
                {
                    COM_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    COM_COM = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    COM_FEC_CRE = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    COM_FEC_MOD = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    FRI_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    USU_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_COMENTARIOS", x => x.COM_ID);
                    table.ForeignKey(
                        name: "FK_COMENTARIOS_FRICCION_FRI_ID",
                        column: x => x.FRI_ID,
                        principalTable: "FRICCION",
                        principalColumn: "FRI_ID");
                    table.ForeignKey(
                        name: "FK_COMENTARIOS_USUARIO_USU_ID",
                        column: x => x.USU_ID,
                        principalTable: "USUARIO",
                        principalColumn: "USU_ID");
                });

            migrationBuilder.CreateTable(
                name: "SOLUCIONES",
                columns: table => new
                {
                    SOL_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SOL_NOM = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SOL_DES = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    SOL_TIE_RES = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    SOL_NIV_EFE = table.Column<int>(type: "int", nullable: true),
                    SOL_EST = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SOL_FEC_CRE = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    SOL_FEC_MOD = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    FRI_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    USU_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SOLUCIONES", x => x.SOL_ID);
                    table.ForeignKey(
                        name: "FK_SOLUCIONES_FRICCION_FRI_ID",
                        column: x => x.FRI_ID,
                        principalTable: "FRICCION",
                        principalColumn: "FRI_ID");
                    table.ForeignKey(
                        name: "FK_SOLUCIONES_USUARIO_USU_ID",
                        column: x => x.USU_ID,
                        principalTable: "USUARIO",
                        principalColumn: "USU_ID");
                });

            migrationBuilder.CreateTable(
                name: "BITACORA_SOLUCIONES",
                columns: table => new
                {
                    BIT_SOL_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BIT_SOL_NOM = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    BIT_SOL_EST = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BIT_SOL_DES = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    BIT_SOL_TIE_TOT_TRA = table.Column<int>(type: "int", nullable: true),
                    BIT_SOL_FEC_CRE = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    BIT_SOL_FEC_MOD = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    SOL_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    USU_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BITACORA_SOLUCIONES", x => x.BIT_SOL_ID);
                    table.ForeignKey(
                        name: "FK_BITACORA_SOLUCIONES_SOLUCIONES_SOL_ID",
                        column: x => x.SOL_ID,
                        principalTable: "SOLUCIONES",
                        principalColumn: "SOL_ID");
                    table.ForeignKey(
                        name: "FK_BITACORA_SOLUCIONES_USUARIO_USU_ID",
                        column: x => x.USU_ID,
                        principalTable: "USUARIO",
                        principalColumn: "USU_ID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_BITACORA_FRICCIONES_FRI_ID",
                table: "BITACORA_FRICCIONES",
                column: "FRI_ID");

            migrationBuilder.CreateIndex(
                name: "IX_BITACORA_FRICCIONES_USU_ID",
                table: "BITACORA_FRICCIONES",
                column: "USU_ID");

            migrationBuilder.CreateIndex(
                name: "IX_BITACORA_SOLUCIONES_SOL_ID",
                table: "BITACORA_SOLUCIONES",
                column: "SOL_ID");

            migrationBuilder.CreateIndex(
                name: "IX_BITACORA_SOLUCIONES_USU_ID",
                table: "BITACORA_SOLUCIONES",
                column: "USU_ID");

            migrationBuilder.CreateIndex(
                name: "IX_COMENTARIOS_FRI_ID",
                table: "COMENTARIOS",
                column: "FRI_ID");

            migrationBuilder.CreateIndex(
                name: "IX_COMENTARIOS_USU_ID",
                table: "COMENTARIOS",
                column: "USU_ID");

            migrationBuilder.CreateIndex(
                name: "IX_DETALLES_PROYECTO_PRO_ID",
                table: "DETALLES_PROYECTO",
                column: "PRO_ID");

            migrationBuilder.CreateIndex(
                name: "IX_DETALLES_PROYECTO_ROL_ID",
                table: "DETALLES_PROYECTO",
                column: "ROL_ID");

            migrationBuilder.CreateIndex(
                name: "IX_DETALLES_PROYECTO_USU_ID",
                table: "DETALLES_PROYECTO",
                column: "USU_ID");

            migrationBuilder.CreateIndex(
                name: "IX_DOCUMENTACION_TAR_ID",
                table: "DOCUMENTACION",
                column: "TAR_ID");

            migrationBuilder.CreateIndex(
                name: "IX_DOCUMENTACION_USU_ID",
                table: "DOCUMENTACION",
                column: "USU_ID");

            migrationBuilder.CreateIndex(
                name: "IX_FRICCION_TAR_ID",
                table: "FRICCION",
                column: "TAR_ID");

            migrationBuilder.CreateIndex(
                name: "IX_FRICCION_USU_ID",
                table: "FRICCION",
                column: "USU_ID");

            migrationBuilder.CreateIndex(
                name: "IX_NOTIFICACIONES_USU_ID",
                table: "NOTIFICACIONES",
                column: "USU_ID");

            migrationBuilder.CreateIndex(
                name: "IX_PROCESO_PRO_ID",
                table: "PROCESO",
                column: "PRO_ID");

            migrationBuilder.CreateIndex(
                name: "IX_PROYECTO_ARE_ID",
                table: "PROYECTO",
                column: "ARE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_SOLUCIONES_FRI_ID",
                table: "SOLUCIONES",
                column: "FRI_ID");

            migrationBuilder.CreateIndex(
                name: "IX_SOLUCIONES_USU_ID",
                table: "SOLUCIONES",
                column: "USU_ID");

            migrationBuilder.CreateIndex(
                name: "IX_TAREA_PRI_ID",
                table: "TAREA",
                column: "PRI_ID");

            migrationBuilder.CreateIndex(
                name: "IX_TAREA_PROC_ID",
                table: "TAREA",
                column: "PROC_ID");

            migrationBuilder.CreateIndex(
                name: "IX_TAREA_USU_ID",
                table: "TAREA",
                column: "USU_ID");

            migrationBuilder.CreateIndex(
                name: "IX_USUARIO_ROL_ID",
                table: "USUARIO",
                column: "ROL_ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BITACORA_FRICCIONES");

            migrationBuilder.DropTable(
                name: "BITACORA_SOLUCIONES");

            migrationBuilder.DropTable(
                name: "COMENTARIOS");

            migrationBuilder.DropTable(
                name: "DETALLES_PROYECTO");

            migrationBuilder.DropTable(
                name: "DOCUMENTACION");

            migrationBuilder.DropTable(
                name: "NOTIFICACIONES");

            migrationBuilder.DropTable(
                name: "SOLUCIONES");

            migrationBuilder.DropTable(
                name: "FRICCION");

            migrationBuilder.DropTable(
                name: "TAREA");

            migrationBuilder.DropTable(
                name: "PRIORIDAD");

            migrationBuilder.DropTable(
                name: "PROCESO");

            migrationBuilder.DropTable(
                name: "USUARIO");

            migrationBuilder.DropTable(
                name: "PROYECTO");

            migrationBuilder.DropTable(
                name: "ROLES");

            migrationBuilder.DropTable(
                name: "AREA");
        }
    }
}
