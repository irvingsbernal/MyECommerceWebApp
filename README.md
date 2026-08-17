# MyECommerceWebApp

Tienda de demostracion con backend .NET 9 (Repository / Unit of Work + EF Core) y frontend Angular 22. El modelo de datos sigue `script_master.sql`: clientes, productos, ordenes, pagos y bitacora.

## Resumen ejecutivo

Un cliente se registra o se identifica por email, elige productos y procesa una compra. El API valida datos, simula el pago, descuenta stock de forma atomica (evita overselling si dos compras coinciden) y deja rastro en bitacora. Un admin demo consulta los eventos.

Datos de prueba al arrancar:

| Recurso | Ejemplo |
|---|---|
| Cliente | `juan.perez@email.com`, `maria.lopez@email.com` |
| Pago autorizado | referencia `VISA-4532` |
| Pago rechazado | referencia `CARD-0000` |
| Pago pendiente | producto `Servidor Enterprise` (precio > 10000) |
| Carrera de stock | producto `Edicion limitada` (stock 1) |
| Admin | clave `demo-admin` |

## Como levantarlo

### API (LocalDB)

```bash
cd MyECommerceWebApp
dotnet run --launch-profile http
```

Swagger: http://localhost:5217/swagger

### Frontend

```bash
cd MyECommerceWebApp.Application
npm start
```

UI: http://localhost:4200 (apunta a http://localhost:5217)

### Tests

```bash
dotnet test
```

### Docker

```bash
docker compose up --build
```

API: http://localhost:8080/swagger

## Guía de evaluación

El pago no llama a un banco ni a una pasarela. El resultado lo deciden la **referencia** y el **monto** (simulador en Application).

### Recorrido en la UI (http://localhost:4200)

1. En `/registro`, pestaña Identificar, usa `juan.perez@email.com` y continúa.
2. En Checkout, elige mouse o teclado y deja la referencia `VISA-4532`. Procesar compra: orden **confirmada**, pago **autorizado** y stock descontado.
3. Repite la compra con referencia `CARD-0000`: pago **rechazado** y orden **rechazada**.
4. Compra solo `Servidor Enterprise` (precio mayor a 10000): pago **pendiente**, orden sigue **pendiente** y no se descuenta stock.
5. Opcional: `Edicion limitada` (stock 1) en dos sesiones a la vez (Juan y Maria). Una compra confirma; la otra responde 409.
6. En `/registro`, pestaña Admin, clave `demo-admin`. Abre `/bitacora` y verifica eventos `PAGO`, `INVENTARIO` o `ERROR`.

### Reglas del simulador

| Condición | Estado del pago | Estado de la orden |
|---|---|---|
| La referencia termina en `0000` | `rechazado` | `rechazada` |
| El total es mayor a 10000 | `pendiente` | `pendiente` (sin movimiento de stock) |
| Cualquier otro caso | `autorizado` | `confirmada` (se descuenta stock) |

La misma prueba por API está en Swagger (http://localhost:5217/swagger) o en [postman/MyECommerceWebApp.postman_collection.json](postman/MyECommerceWebApp.postman_collection.json).

## Arquitectura tecnica

Capas en un solo proyecto API:

- **Domain**: entidades y contratos (`IRepository<T>`, `IUnitOfWork`, repositorios especificos).
- **Application**: casos de uso, FluentValidation, simulador de pago.
- **Infrastructure**: EF Core, repositorios, JWT, seed.
- **API**: controladores, middleware de errores, Swagger.

Detalle: [docs/arquitectura.md](docs/arquitectura.md)

### Endpoints principales

| Metodo | Ruta | Descripcion |
|---|---|---|
| POST | `/api/clientes` | Registro + JWT |
| POST | `/api/auth/identificar` | Email de cliente + JWT |
| POST | `/api/auth/admin` | Admin demo |
| GET | `/api/productos` | Catalogo |
| POST | `/api/compras` | Orden + pago + inventario (transaccion) |
| GET | `/api/ordenes/{id}` | Estado |
| POST | `/api/ordenes/{id}/pagos` | Pago simulado |
| POST | `/api/ordenes/{id}/pagos/reintentar` | Reintento (max 3) |
| POST | `/api/ordenes/{id}/inventario` | Descuento atomico de stock |
| GET | `/api/logs` | Bitacora (admin) |

Coleccion Postman: [postman/MyECommerceWebApp.postman_collection.json](postman/MyECommerceWebApp.postman_collection.json)

### Concurrencia de stock

Sin cambiar el esquema, el descuento es:

`UPDATE Productos SET Stock = Stock - n WHERE ProductoId = @id AND Stock >= n`

Cero filas = 409 Conflict y rollback de la compra completa.

### Secure SDLC

JWT con clave por configuracion/entorno, CORS acotado a Angular, validacion Data Annotations + FluentValidation, consultas parametrizadas via EF, secretos de Docker por variables de entorno.

Registro de lo generado/modificado/descartado con IA: [docs/REGISTRO_IA.md](docs/REGISTRO_IA.md)
