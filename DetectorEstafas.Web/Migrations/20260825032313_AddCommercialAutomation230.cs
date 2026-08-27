using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DetectorEstafas.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddCommercialAutomation230 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "ApiClientes",
                type: "nvarchar(254)",
                maxLength: 254,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ApiClaveEntregas",
                columns: table => new
                {
                    ApiClaveEntregaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApiClaveId = table.Column<int>(type: "int", nullable: false),
                    TokenHash = table.Column<byte[]>(type: "binary(32)", nullable: false),
                    ClaveProtegida = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    FechaCreacionUtc = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    FechaExpiracionUtc = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    FechaConsumoUtc = table.Column<DateTime>(type: "datetime2(0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiClaveEntregas", x => x.ApiClaveEntregaId);
                    table.ForeignKey(
                        name: "FK_ApiClaveEntregas_ApiClaves_ApiClaveId",
                        column: x => x.ApiClaveId,
                        principalTable: "ApiClaves",
                        principalColumn: "ApiClaveId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SuscripcionesComerciales",
                columns: table => new
                {
                    SuscripcionComercialId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReferenciaPublica = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: false),
                    Plan = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    MercadoPagoPreapprovalId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MercadoPagoInitPoint = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Monto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Moneda = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    ApiClienteId = table.Column<int>(type: "int", nullable: true),
                    FechaCreacionUtc = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    FechaActualizacionUtc = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    FechaUltimoPagoUtc = table.Column<DateTime>(type: "datetime2(0)", nullable: true),
                    ProximaRenovacionUtc = table.Column<DateTime>(type: "datetime2(0)", nullable: true),
                    PeriodoGraciaHastaUtc = table.Column<DateTime>(type: "datetime2(0)", nullable: true),
                    FechaCancelacionUtc = table.Column<DateTime>(type: "datetime2(0)", nullable: true),
                    FechaFinAccesoUtc = table.Column<DateTime>(type: "datetime2(0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SuscripcionesComerciales", x => x.SuscripcionComercialId);
                    table.ForeignKey(
                        name: "FK_SuscripcionesComerciales_ApiClientes_ApiClienteId",
                        column: x => x.ApiClienteId,
                        principalTable: "ApiClientes",
                        principalColumn: "ApiClienteId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "WebhookComercialEventos",
                columns: table => new
                {
                    WebhookComercialEventoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SuscripcionComercialId = table.Column<int>(type: "int", nullable: true),
                    Proveedor = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    EventoProveedorId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    RecursoId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Accion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FechaProcesadoUtc = table.Column<DateTime>(type: "datetime2(0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebhookComercialEventos", x => x.WebhookComercialEventoId);
                    table.ForeignKey(
                        name: "FK_WebhookComercialEventos_SuscripcionesComerciales_SuscripcionComercialId",
                        column: x => x.SuscripcionComercialId,
                        principalTable: "SuscripcionesComerciales",
                        principalColumn: "SuscripcionComercialId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApiClientes_Email",
                table: "ApiClientes",
                column: "Email",
                unique: true,
                filter: "[Email] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ApiClaveEntregas_ApiClaveId",
                table: "ApiClaveEntregas",
                column: "ApiClaveId");

            migrationBuilder.CreateIndex(
                name: "IX_ApiClaveEntregas_TokenHash",
                table: "ApiClaveEntregas",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SuscripcionesComerciales_ApiClienteId",
                table: "SuscripcionesComerciales",
                column: "ApiClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_SuscripcionesComerciales_Email_Estado",
                table: "SuscripcionesComerciales",
                columns: new[] { "Email", "Estado" });

            migrationBuilder.CreateIndex(
                name: "IX_SuscripcionesComerciales_MercadoPagoPreapprovalId",
                table: "SuscripcionesComerciales",
                column: "MercadoPagoPreapprovalId",
                unique: true,
                filter: "[MercadoPagoPreapprovalId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SuscripcionesComerciales_ReferenciaPublica",
                table: "SuscripcionesComerciales",
                column: "ReferenciaPublica",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WebhookComercialEventos_Proveedor_EventoProveedorId",
                table: "WebhookComercialEventos",
                columns: new[] { "Proveedor", "EventoProveedorId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WebhookComercialEventos_SuscripcionComercialId",
                table: "WebhookComercialEventos",
                column: "SuscripcionComercialId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApiClaveEntregas");

            migrationBuilder.DropTable(
                name: "WebhookComercialEventos");

            migrationBuilder.DropTable(
                name: "SuscripcionesComerciales");

            migrationBuilder.DropIndex(
                name: "IX_ApiClientes_Email",
                table: "ApiClientes");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "ApiClientes");
        }
    }
}
