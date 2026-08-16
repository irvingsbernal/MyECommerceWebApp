using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyECommerceWebApp.Domain.Entities;

namespace MyECommerceWebApp.Infrastructure.Persistence.Configurations;

public class OrdenConfiguration : IEntityTypeConfiguration<Orden>
{
    public void Configure(EntityTypeBuilder<Orden> builder)
    {
        builder.ToTable("Ordenes", table =>
        {
            table.HasCheckConstraint("CK_Ordenes_Estado", "[Estado] IN ('pendiente', 'confirmada', 'cancelada', 'rechazada')");
            table.HasCheckConstraint("CK_Ordenes_Total", "[Total] >= 0");
        });
        builder.HasKey(orden => orden.OrdenId);

        builder.Property(orden => orden.FechaOrden).HasColumnType("datetime2(0)");
        builder.Property(orden => orden.Estado).IsRequired().HasMaxLength(20);
        builder.Property(orden => orden.Total).HasColumnType("decimal(18,2)");
        builder.Property(orden => orden.Observaciones).HasMaxLength(500);

        builder.HasOne(orden => orden.Cliente)
            .WithMany(cliente => cliente.Ordenes)
            .HasForeignKey(orden => orden.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(orden => orden.Estado).HasDatabaseName("IX_Ordenes_Estado");
        builder.HasIndex(orden => new { orden.ClienteId, orden.FechaOrden }).HasDatabaseName("IX_Ordenes_ClienteId");
    }
}
