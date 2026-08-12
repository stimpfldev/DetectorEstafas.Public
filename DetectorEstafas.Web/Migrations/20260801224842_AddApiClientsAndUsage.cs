using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DetectorEstafas.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddApiClientsAndUsage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApiClientes",
                columns: table => new
                {
                    ApiClienteId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Plan = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CuotaDiaria = table.Column<int>(type: "int", nullable: false),
                    Habilitado = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacionUtc = table.Column<DateTime>(type: "datetime2(0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiClientes", x => x.ApiClienteId);
                });

            migrationBuilder.CreateTable(
                name: "ApiClaves",
                columns: table => new
                {
                    ApiClaveId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApiClienteId = table.Column<int>(type: "int", nullable: false),
                    Prefijo = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: false),
                    Hash = table.Column<byte[]>(type: "binary(32)", nullable: false),
                    Habilitada = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacionUtc = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    FechaRevocacionUtc = table.Column<DateTime>(type: "datetime2(0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiClaves", x => x.ApiClaveId);
                    table.ForeignKey(
                        name: "FK_ApiClaves_ApiClientes_ApiClienteId",
                        column: x => x.ApiClienteId,
                        principalTable: "ApiClientes",
                        principalColumn: "ApiClienteId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ApiConsumosDiarios",
                columns: table => new
                {
                    ApiConsumoDiarioId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApiClienteId = table.Column<int>(type: "int", nullable: false),
                    FechaUtc = table.Column<DateOnly>(type: "date", nullable: false),
                    CantidadSolicitudes = table.Column<int>(type: "int", nullable: false),
                    UltimaSolicitudUtc = table.Column<DateTime>(type: "datetime2(0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiConsumosDiarios", x => x.ApiConsumoDiarioId);
                    table.ForeignKey(
                        name: "FK_ApiConsumosDiarios_ApiClientes_ApiClienteId",
                        column: x => x.ApiClienteId,
                        principalTable: "ApiClientes",
                        principalColumn: "ApiClienteId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApiClaves_ApiClienteId_Habilitada",
                table: "ApiClaves",
                columns: new[] { "ApiClienteId", "Habilitada" });

            migrationBuilder.CreateIndex(
                name: "IX_ApiClaves_Prefijo",
                table: "ApiClaves",
                column: "Prefijo");

            migrationBuilder.CreateIndex(
                name: "IX_ApiClientes_Nombre",
                table: "ApiClientes",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApiConsumosDiarios_ApiClienteId_FechaUtc",
                table: "ApiConsumosDiarios",
                columns: new[] { "ApiClienteId", "FechaUtc" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApiClaves");

            migrationBuilder.DropTable(
                name: "ApiConsumosDiarios");

            migrationBuilder.DropTable(
                name: "ApiClientes");
        }
    }
}
