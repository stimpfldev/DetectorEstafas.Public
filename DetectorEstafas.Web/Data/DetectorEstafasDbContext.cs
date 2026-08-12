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
    }
}
