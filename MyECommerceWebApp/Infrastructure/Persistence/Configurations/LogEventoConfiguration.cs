using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyECommerceWebApp.Domain.Entities;

namespace MyECommerceWebApp.Infrastructure.Persistence.Configurations;

public class LogEventoConfiguration : IEntityTypeConfiguration<LogEvento>
{
    public void Configure(EntityTypeBuilder<LogEvento> builder)
    {
        builder.ToTable("Logs", table =>
        {
            table.HasCheckConstraint(
                "CK_Logs_Operacion",
                "[Operacion] IN ('INSERT', 'UPDATE', 'DELETE', 'ERROR', 'PAGO', 'INVENTARIO')");
        });
        builder.HasKey(log => log.LogId);

        builder.Property(log => log.TablaAfectada).IsRequired().HasMaxLength(100);
        builder.Property(log => log.Operacion).IsRequired().HasMaxLength(20);
        builder.Property(log => log.RegistroId).HasMaxLength(50);
        builder.Property(log => log.MensajeLog).HasColumnType("nvarchar(max)");
        builder.Property(log => log.Usuario).IsRequired().HasMaxLength(128).HasDefaultValue("api");
        builder.Property(log => log.FechaEvento).HasColumnType("datetime2(0)");

        builder.HasIndex(log => log.FechaEvento).HasDatabaseName("IX_Logs_FechaEvento");
    }
}
