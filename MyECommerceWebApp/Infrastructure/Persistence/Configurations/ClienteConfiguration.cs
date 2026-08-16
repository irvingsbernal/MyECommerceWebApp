using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyECommerceWebApp.Domain.Entities;

namespace MyECommerceWebApp.Infrastructure.Persistence.Configurations;

public class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        builder.ToTable("Clientes", table =>
        {
            table.HasCheckConstraint("CK_Clientes_Email", "[Email] LIKE '%_@_%._%'");
            table.HasCheckConstraint("CK_Clientes_Nombre", "LEN(LTRIM(RTRIM([Nombre]))) > 0");
            table.HasCheckConstraint("CK_Clientes_Apellido", "LEN(LTRIM(RTRIM([Apellido]))) > 0");
        });
        builder.HasKey(cliente => cliente.ClienteId);

        builder.Property(cliente => cliente.Nombre).IsRequired().HasMaxLength(100);
        builder.Property(cliente => cliente.Apellido).IsRequired().HasMaxLength(100);
        builder.Property(cliente => cliente.Email).IsRequired().HasMaxLength(255);
        builder.Property(cliente => cliente.Telefono).HasMaxLength(20);
        builder.Property(cliente => cliente.Direccion).IsRequired().HasMaxLength(300);
        builder.Property(cliente => cliente.FechaRegistro).HasColumnType("datetime2(0)");
        builder.Property(cliente => cliente.Activo).HasDefaultValue(true);

        builder.HasIndex(cliente => cliente.Email).IsUnique().HasDatabaseName("UQ_Clientes_Email");
    }
}
