using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DetectorEstafas.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddMonthlyApiQuotas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CuotaMensual",
                table: "ApiClientes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaInicioPlanUtc",
                table: "ApiClientes",
                type: "datetime2(0)",
                nullable: true);

            // Clientes de prueba existentes:
            // se normalizan al plan definitivo de 200 análisis diarios.
            migrationBuilder.Sql("""
                UPDATE ApiClientes
                SET
                    CuotaDiaria = 200,
                    CuotaMensual = NULL,
                    FechaInicioPlanUtc = FechaCreacionUtc
                WHERE LOWER(LTRIM(RTRIM([Plan]))) = 'prueba';
                """);

            // El antiguo plan "Comercial" pasa a Starter.
            // Se conserva CuotaDiaria como dato histórico aunque
            // el nuevo sistema ya no la utiliza para planes pagos.
            migrationBuilder.Sql("""
                UPDATE ApiClientes
                SET
                    [Plan] = 'Starter',
                    CuotaMensual = 5000,
                    FechaInicioPlanUtc = SYSUTCDATETIME()
                WHERE LOWER(LTRIM(RTRIM([Plan]))) = 'comercial';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Permite volver a una versión 2.0.x sin dejar planes
            // que esa versión no reconoce.
            migrationBuilder.Sql("""
                UPDATE ApiClientes
                SET
                    [Plan] = 'Comercial',
                    CuotaDiaria =
                        CASE
                            WHEN CuotaDiaria < 1 THEN 200
                            ELSE CuotaDiaria
                        END
                WHERE [Plan] IN ('Starter', 'Growth', 'A medida');
                """);

            migrationBuilder.DropColumn(
                name: "CuotaMensual",
                table: "ApiClientes");

            migrationBuilder.DropColumn(
                name: "FechaInicioPlanUtc",
                table: "ApiClientes");
        }
    }
}