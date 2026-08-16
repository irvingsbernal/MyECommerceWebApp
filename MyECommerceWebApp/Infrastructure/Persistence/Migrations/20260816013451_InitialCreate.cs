using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyECommerceWebApp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Clientes",
                columns: table => new
                {
                    ClienteId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Apellido = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Direccion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clientes", x => x.ClienteId);
                    table.CheckConstraint("CK_Clientes_Apellido", "LEN(LTRIM(RTRIM([Apellido]))) > 0");
                    table.CheckConstraint("CK_Clientes_Email", "[Email] LIKE '%_@_%._%'");
                    table.CheckConstraint("CK_Clientes_Nombre", "LEN(LTRIM(RTRIM([Nombre]))) > 0");
                });

            migrationBuilder.CreateTable(
                name: "Logs",
                columns: table => new
                {
                    LogId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TablaAfectada = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Operacion = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RegistroId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MensajeLog = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Usuario = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false, defaultValue: "api"),
                    FechaEvento = table.Column<DateTime>(type: "datetime2(0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Logs", x => x.LogId);
                    table.CheckConstraint("CK_Logs_Operacion", "[Operacion] IN ('INSERT', 'UPDATE', 'DELETE', 'ERROR', 'PAGO', 'INVENTARIO')");
                });

            migrationBuilder.CreateTable(
                name: "Productos",
                columns: table => new
                {
                    ProductoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Precio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Stock = table.Column<int>(type: "int", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Productos", x => x.ProductoId);
                    table.CheckConstraint("CK_Productos_Precio", "[Precio] > 0");
                    table.CheckConstraint("CK_Productos_Stock", "[Stock] >= 0");
                });

            migrationBuilder.CreateTable(
                name: "Ordenes",
                columns: table => new
                {
                    OrdenId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    FechaOrden = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ordenes", x => x.OrdenId);
                    table.CheckConstraint("CK_Ordenes_Estado", "[Estado] IN ('pendiente', 'confirmada', 'cancelada', 'rechazada')");
                    table.CheckConstraint("CK_Ordenes_Total", "[Total] >= 0");
                    table.ForeignKey(
                        name: "FK_Ordenes_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "ClienteId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrdenDetalle",
                columns: table => new
                {
                    OrdenDetalleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrdenId = table.Column<int>(type: "int", nullable: false),
                    ProductoId = table.Column<int>(type: "int", nullable: false),
                    Cantidad = table.Column<int>(type: "int", nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false, computedColumnSql: "[Cantidad] * [PrecioUnitario]", stored: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrdenDetalle", x => x.OrdenDetalleId);
                    table.CheckConstraint("CK_OrdenDetalle_Cantidad", "[Cantidad] > 0");
                    table.CheckConstraint("CK_OrdenDetalle_PrecioUnitario", "[PrecioUnitario] > 0");
                    table.ForeignKey(
                        name: "FK_OrdenDetalle_Ordenes_OrdenId",
                        column: x => x.OrdenId,
                        principalTable: "Ordenes",
                        principalColumn: "OrdenId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrdenDetalle_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "ProductoId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Pagos",
                columns: table => new
                {
                    PagoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrdenId = table.Column<int>(type: "int", nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MetodoPago = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Referencia = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Intentos = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    FechaPago = table.Column<DateTime>(type: "datetime2(0)", nullable: true),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    MensajeError = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pagos", x => x.PagoId);
                    table.CheckConstraint("CK_Pagos_Estado", "[Estado] IN ('autorizado', 'rechazado', 'pendiente')");
                    table.CheckConstraint("CK_Pagos_Intentos", "[Intentos] >= 0");
                    table.CheckConstraint("CK_Pagos_Monto", "[Monto] > 0");
                    table.ForeignKey(
                        name: "FK_Pagos_Ordenes_OrdenId",
                        column: x => x.OrdenId,
                        principalTable: "Ordenes",
                        principalColumn: "OrdenId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "UQ_Clientes_Email",
                table: "Clientes",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Logs_FechaEvento",
                table: "Logs",
                column: "FechaEvento");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenDetalle_ProductoId",
                table: "OrdenDetalle",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "UQ_OrdenDetalle_Orden_Producto",
                table: "OrdenDetalle",
                columns: new[] { "OrdenId", "ProductoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ordenes_ClienteId",
                table: "Ordenes",
                columns: new[] { "ClienteId", "FechaOrden" });

            migrationBuilder.CreateIndex(
                name: "IX_Ordenes_Estado",
                table: "Ordenes",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_Estado",
                table: "Pagos",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_OrdenId",
                table: "Pagos",
                column: "OrdenId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Logs");

            migrationBuilder.DropTable(
                name: "OrdenDetalle");

            migrationBuilder.DropTable(
                name: "Pagos");

            migrationBuilder.DropTable(
                name: "Productos");

            migrationBuilder.DropTable(
                name: "Ordenes");

            migrationBuilder.DropTable(
                name: "Clientes");
        }
    }
}
