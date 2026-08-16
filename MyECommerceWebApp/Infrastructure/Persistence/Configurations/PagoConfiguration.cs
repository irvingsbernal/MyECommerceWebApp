using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyECommerceWebApp.Domain.Entities;

namespace MyECommerceWebApp.Infrastructure.Persistence.Configurations;

public class PagoConfiguration : IEntityTypeConfiguration<Pago>
{
    public void Configure(EntityTypeBuilder<Pago> builder)
    {
        builder.ToTable("Pagos", table =>
        {
            table.HasCheckConstraint("CK_Pagos_Estado", "[Estado] IN ('autorizado', 'rechazado', 'pendiente')");
            table.HasCheckConstraint("CK_Pagos_Monto", "[Monto] > 0");
            table.HasCheckConstraint("CK_Pagos_Intentos", "[Intentos] >= 0");
        });
        builder.HasKey(pago => pago.PagoId);

        builder.Property(pago => pago.Monto).HasColumnType("decimal(18,2)");
        builder.Property(pago => pago.Estado).IsRequired().HasMaxLength(20);
        builder.Property(pago => pago.MetodoPago).IsRequired().HasMaxLength(50);
        builder.Property(pago => pago.Referencia).HasMaxLength(100);
        builder.Property(pago => pago.Intentos).HasDefaultValue(0);
        builder.Property(pago => pago.FechaPago).HasColumnType("datetime2(0)");
        builder.Property(pago => pago.FechaRegistro).HasColumnType("datetime2(0)");
        builder.Property(pago => pago.MensajeError).HasMaxLength(500);

        builder.HasOne(pago => pago.Orden)
            .WithMany(orden => orden.Pagos)
            .HasForeignKey(pago => pago.OrdenId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(pago => pago.Estado).HasDatabaseName("IX_Pagos_Estado");
    }
}
