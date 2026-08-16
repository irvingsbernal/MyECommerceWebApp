using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyECommerceWebApp.Domain.Entities;

namespace MyECommerceWebApp.Infrastructure.Persistence.Configurations;

public class ProductoConfiguration : IEntityTypeConfiguration<Producto>
{
    public void Configure(EntityTypeBuilder<Producto> builder)
    {
        builder.ToTable("Productos", table =>
        {
            table.HasCheckConstraint("CK_Productos_Precio", "[Precio] > 0");
            table.HasCheckConstraint("CK_Productos_Stock", "[Stock] >= 0");
        });
        builder.HasKey(producto => producto.ProductoId);

        builder.Property(producto => producto.Nombre).IsRequired().HasMaxLength(200);
        builder.Property(producto => producto.Descripcion).HasMaxLength(500);
        builder.Property(producto => producto.Precio).HasColumnType("decimal(18,2)");
        builder.Property(producto => producto.Activo).HasDefaultValue(true);
    }
}
