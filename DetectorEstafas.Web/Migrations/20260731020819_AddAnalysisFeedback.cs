using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DetectorEstafas.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddAnalysisFeedback : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AnalisisFeedbacks",
                columns: table => new
                {
                    AnalisisRegistroId = table.Column<long>(type: "bigint", nullable: false),
                    FueUtil = table.Column<bool>(type: "bit", nullable: false),
                    FechaUtc = table.Column<DateTime>(type: "datetime2(0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnalisisFeedbacks", x => x.AnalisisRegistroId);
                    table.ForeignKey(
                        name: "FK_AnalisisFeedbacks_AnalisisRegistros_AnalisisRegistroId",
                        column: x => x.AnalisisRegistroId,
                        principalTable: "AnalisisRegistros",
                        principalColumn: "AnalisisRegistroId",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnalisisFeedbacks");
        }
    }
}
