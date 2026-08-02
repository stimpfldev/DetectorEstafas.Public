using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DetectorEstafas.Web.Migrations
{
    /// <inheritdoc />
    public partial class InitialMetrics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AnalisisRegistros",
                columns: table => new
                {
                    AnalisisRegistroId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FechaUtc = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    TipoContenido = table.Column<int>(type: "int", nullable: false),
                    NivelRiesgo = table.Column<int>(type: "int", nullable: false),
                    Puntaje = table.Column<byte>(type: "tinyint", nullable: false),
                    CantidadSenales = table.Column<short>(type: "smallint", nullable: false),
                    Origen = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnalisisRegistros", x => x.AnalisisRegistroId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnalisisRegistros_FechaUtc",
                table: "AnalisisRegistros",
                column: "FechaUtc");

            migrationBuilder.CreateIndex(
                name: "IX_AnalisisRegistros_TipoContenido_NivelRiesgo",
                table: "AnalisisRegistros",
                columns: new[] { "TipoContenido", "NivelRiesgo" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnalisisRegistros");
        }
    }
}
