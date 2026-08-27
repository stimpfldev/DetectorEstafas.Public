using DetectorEstafas.Web.Models;
using DetectorEstafas.Web.Models.ApiComercial;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DetectorEstafas.Web.Data;

public class DetectorEstafasDbContext :
    IdentityDbContext<UsuarioAplicacion>
{
    public DetectorEstafasDbContext(
        DbContextOptions<DetectorEstafasDbContext> options)
        : base(options)
    {
    }

    public DbSet<AnalisisRegistro> AnalisisRegistros =>
        Set<AnalisisRegistro>();

    public DbSet<AnalisisFeedback> AnalisisFeedbacks =>
        Set<AnalisisFeedback>();

    public DbSet<ReporteComunitario> ReportesComunitarios =>
        Set<ReporteComunitario>();

    public DbSet<ApiCliente> ApiClientes =>
        Set<ApiCliente>();

    public DbSet<ApiClave> ApiClaves =>
        Set<ApiClave>();

    public DbSet<ApiConsumoDiario> ApiConsumosDiarios =>
        Set<ApiConsumoDiario>();

    public DbSet<SuscripcionComercial> SuscripcionesComerciales =>
        Set<SuscripcionComercial>();

    public DbSet<WebhookComercialEvento> WebhookComercialEventos =>
        Set<WebhookComercialEvento>();

    public DbSet<ApiClaveEntrega> ApiClaveEntregas =>
        Set<ApiClaveEntrega>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AnalisisRegistro>(entity =>
        {
            entity.ToTable("AnalisisRegistros");

            entity.HasKey(registro =>
                registro.AnalisisRegistroId);

            entity.Property(registro =>
                    registro.FechaUtc)
                .HasColumnType("datetime2(0)")
                .IsRequired();

            entity.Property(registro =>
                    registro.TipoContenido)
                .IsRequired();

            entity.Property(registro =>
                    registro.NivelRiesgo)
                .IsRequired();

            entity.Property(registro =>
                    registro.Puntaje)
                .HasColumnType("tinyint")
                .IsRequired();

            entity.Property(registro =>
                    registro.CantidadSenales)
                .HasColumnType("smallint")
                .IsRequired();

            entity.Property(registro =>
                    registro.Origen)
                .HasMaxLength(20)
                .IsUnicode(false)
                .IsRequired();

            entity.HasIndex(registro =>
                registro.FechaUtc);

            entity.HasIndex(registro => new
            {
                registro.TipoContenido,
                registro.NivelRiesgo
            });
        });

        modelBuilder.Entity<AnalisisFeedback>(entity =>
        {
            entity.ToTable("AnalisisFeedbacks");

            entity.HasKey(feedback =>
                feedback.AnalisisRegistroId);

            entity.Property(feedback =>
                    feedback.FueUtil)
                .IsRequired();

            entity.Property(feedback =>
                    feedback.FechaUtc)
                .HasColumnType("datetime2(0)")
                .IsRequired();

            entity.HasOne<AnalisisRegistro>()
                .WithOne()
                .HasForeignKey<AnalisisFeedback>(
                    feedback =>
                        feedback.AnalisisRegistroId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ReporteComunitario>(entity =>
        {
            entity.ToTable("ReportesComunitarios");

            entity.HasKey(reporte =>
                reporte.AnalisisRegistroId);

            entity.Property(reporte =>
                    reporte.Categoria)
                .IsRequired();

            entity.Property(reporte =>
                    reporte.FechaUtc)
                .HasColumnType("datetime2(0)")
                .IsRequired();

            entity.HasOne<AnalisisRegistro>()
                .WithOne()
                .HasForeignKey<ReporteComunitario>(
                    reporte =>
                        reporte.AnalisisRegistroId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(reporte =>
                reporte.Categoria);

            entity.HasIndex(reporte =>
                reporte.FechaUtc);
        });

        modelBuilder.Entity<ApiCliente>(entity =>
        {
            entity.ToTable("ApiClientes");
            entity.HasKey(item =>
                item.ApiClienteId);

            entity.Property(item => item.Nombre)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(item => item.Email)
                .HasMaxLength(254);

            entity.Property(item => item.Plan)
                .HasMaxLength(40)
                .IsRequired();

            entity.Property(item =>
                    item.FechaCreacionUtc)
                .HasColumnType("datetime2(0)")
                .IsRequired();

            entity.Property(item =>
                    item.FechaInicioPlanUtc)
                .HasColumnType("datetime2(0)");

            entity.HasIndex(item => item.Nombre)
                .IsUnique();

            entity.HasIndex(item => item.Email)
                .IsUnique()
                .HasFilter("[Email] IS NOT NULL");
        });

        modelBuilder.Entity<ApiClave>(entity =>
        {
            entity.ToTable("ApiClaves");
            entity.HasKey(item =>
                item.ApiClaveId);

            entity.Property(item => item.Prefijo)
                .HasMaxLength(8)
                .IsUnicode(false)
                .IsRequired();

            entity.Property(item => item.Hash)
                .HasColumnType("binary(32)")
                .IsRequired();

            entity.Property(item =>
                    item.FechaCreacionUtc)
                .HasColumnType("datetime2(0)")
                .IsRequired();

            entity.Property(item =>
                    item.FechaRevocacionUtc)
                .HasColumnType("datetime2(0)");

            entity.HasOne(item => item.Cliente)
                .WithMany(item => item.Claves)
                .HasForeignKey(item =>
                    item.ApiClienteId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(item => item.Prefijo);

            entity.HasIndex(item => new
            {
                item.ApiClienteId,
                item.Habilitada
            });
        });

        modelBuilder.Entity<ApiConsumoDiario>(entity =>
        {
            entity.ToTable("ApiConsumosDiarios");
            entity.HasKey(item =>
                item.ApiConsumoDiarioId);

            entity.Property(item => item.FechaUtc)
                .HasColumnType("date")
                .IsRequired();

            entity.Property(item =>
                    item.UltimaSolicitudUtc)
                .HasColumnType("datetime2(0)")
                .IsRequired();

            entity.HasOne(item => item.Cliente)
                .WithMany(item => item.Consumos)
                .HasForeignKey(item =>
                    item.ApiClienteId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(item => new
            {
                item.ApiClienteId,
                item.FechaUtc
            })
            .IsUnique();
        });

        modelBuilder.Entity<SuscripcionComercial>(entity =>
        {
            entity.ToTable("SuscripcionesComerciales");
            entity.HasKey(item => item.SuscripcionComercialId);

            entity.Property(item => item.Nombre)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(item => item.Email)
                .HasMaxLength(254)
                .IsRequired();

            entity.Property(item => item.Plan)
                .HasMaxLength(40)
                .IsRequired();

            entity.Property(item => item.Estado)
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(item => item.MercadoPagoPreapprovalId)
                .HasMaxLength(100);

            entity.Property(item => item.MercadoPagoInitPoint)
                .HasMaxLength(1000);

            entity.Property(item => item.Monto)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            entity.Property(item => item.Moneda)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsRequired();

            entity.Property(item => item.FechaCreacionUtc)
                .HasColumnType("datetime2(0)")
                .IsRequired();

            entity.Property(item => item.FechaActualizacionUtc)
                .HasColumnType("datetime2(0)")
                .IsRequired();

            entity.Property(item => item.FechaUltimoPagoUtc)
                .HasColumnType("datetime2(0)");

            entity.Property(item => item.ProximaRenovacionUtc)
                .HasColumnType("datetime2(0)");

            entity.Property(item => item.PeriodoGraciaHastaUtc)
                .HasColumnType("datetime2(0)");

            entity.Property(item => item.FechaCancelacionUtc)
                .HasColumnType("datetime2(0)");

            entity.Property(item => item.FechaFinAccesoUtc)
                .HasColumnType("datetime2(0)");

            entity.HasOne(item => item.Cliente)
                .WithMany(item => item.Suscripciones)
                .HasForeignKey(item => item.ApiClienteId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(item => item.ReferenciaPublica)
                .IsUnique();

            entity.HasIndex(item => item.MercadoPagoPreapprovalId)
                .IsUnique()
                .HasFilter("[MercadoPagoPreapprovalId] IS NOT NULL");

            entity.HasIndex(item => new
            {
                item.Email,
                item.Estado
            });
        });

        modelBuilder.Entity<WebhookComercialEvento>(entity =>
        {
            entity.ToTable("WebhookComercialEventos");
            entity.HasKey(item => item.WebhookComercialEventoId);

            entity.Property(item => item.Proveedor)
                .HasMaxLength(40)
                .IsRequired();

            entity.Property(item => item.EventoProveedorId)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(item => item.Tipo)
                .HasMaxLength(80)
                .IsRequired();

            entity.Property(item => item.RecursoId)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(item => item.Accion)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(item => item.FechaProcesadoUtc)
                .HasColumnType("datetime2(0)")
                .IsRequired();

            entity.HasOne(item => item.Suscripcion)
                .WithMany(item => item.Eventos)
                .HasForeignKey(item => item.SuscripcionComercialId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(item => new
            {
                item.Proveedor,
                item.EventoProveedorId
            })
            .IsUnique();
        });

        modelBuilder.Entity<ApiClaveEntrega>(entity =>
        {
            entity.ToTable("ApiClaveEntregas");
            entity.HasKey(item => item.ApiClaveEntregaId);

            entity.Property(item => item.TokenHash)
                .HasColumnType("binary(32)")
                .IsRequired();

            entity.Property(item => item.ClaveProtegida)
                .HasMaxLength(1000)
                .IsRequired();

            entity.Property(item => item.FechaCreacionUtc)
                .HasColumnType("datetime2(0)")
                .IsRequired();

            entity.Property(item => item.FechaExpiracionUtc)
                .HasColumnType("datetime2(0)")
                .IsRequired();

            entity.Property(item => item.FechaConsumoUtc)
                .HasColumnType("datetime2(0)");

            entity.HasOne(item => item.Clave)
                .WithMany(item => item.Entregas)
                .HasForeignKey(item => item.ApiClaveId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(item => item.TokenHash)
                .IsUnique();
        });
    }
}
