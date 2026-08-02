using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DetectorEstafas.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddCommunityReports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReportesComunitarios",
                columns: table => new
                {
                    AnalisisRegistroId = table.Column<long>(type: "bigint", nullable: false),
                    Categoria = table.Column<int>(type: "int", nullable: false),
                    FechaUtc = table.Column<DateTime>(type: "datetime2(0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportesComunitarios", x => x.AnalisisRegistroId);
                    table.ForeignKey(
                        name: "FK_ReportesComunitarios_AnalisisRegistros_AnalisisRegistroId",
                        column: x => x.AnalisisRegistroId,
                        principalTable: "AnalisisRegistros",
                        principalColumn: "AnalisisRegistroId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReportesComunitarios_Categoria",
                table: "ReportesComunitarios",
                column: "Categoria");

            migrationBuilder.CreateIndex(
                name: "IX_ReportesComunitarios_FechaUtc",
                table: "ReportesComunitarios",
                column: "FechaUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReportesComunitarios");
        }
    }
}
