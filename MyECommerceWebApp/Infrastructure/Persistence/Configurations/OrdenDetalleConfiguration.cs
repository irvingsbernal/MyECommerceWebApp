using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyECommerceWebApp.Domain.Entities;

namespace MyECommerceWebApp.Infrastructure.Persistence.Configurations;

public class OrdenDetalleConfiguration : IEntityTypeConfiguration<OrdenDetalle>
{
    public void Configure(EntityTypeBuilder<OrdenDetalle> builder)
    {
        builder.ToTable("OrdenDetalle", table =>
        {
            table.HasCheckConstraint("CK_OrdenDetalle_Cantidad", "[Cantidad] > 0");
            table.HasCheckConstraint("CK_OrdenDetalle_PrecioUnitario", "[PrecioUnitario] > 0");
        });
        builder.HasKey(detalle => detalle.OrdenDetalleId);

        builder.Property(detalle => detalle.PrecioUnitario).HasColumnType("decimal(18,2)");
        builder.Property(detalle => detalle.Subtotal)
            .HasColumnType("decimal(18,2)")
            .HasComputedColumnSql("[Cantidad] * [PrecioUnitario]", stored: true);

        builder.HasOne(detalle => detalle.Orden)
            .WithMany(orden => orden.Detalles)
            .HasForeignKey(detalle => detalle.OrdenId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(detalle => detalle.Producto)
            .WithMany(producto => producto.OrdenDetalles)
            .HasForeignKey(detalle => detalle.ProductoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(detalle => new { detalle.OrdenId, detalle.ProductoId })
            .IsUnique()
            .HasDatabaseName("UQ_OrdenDetalle_Orden_Producto");
    }
}
