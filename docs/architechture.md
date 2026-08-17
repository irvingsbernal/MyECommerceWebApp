# Arquitectura MyECommerceWebApp

Monolito modular: SPA Angular 22, API ASP.NET Core 9 y SQL Server. No hay microservicios, Identity, Redis, SignalR ni pasarela de pago real.


## Diagrama 1 — Contenedores y despliegue

Local: Angular `http://localhost:4200` llama a la API `http://localhost:5217` con CORS. Docker: nginx `:80` sirve la SPA y hace proxy de `/api/` a `http://api:8080/api/`. SQL Server 2019, base `ecommercedev`.

```mermaid
flowchart TB
  Browser["Navegador"]

  subgraph localDev [Desarrollo local]
    SpaDev["Angular 22 ng serve :4200"]
    ApiDev["API .NET 9 :5217"]
  end

  subgraph dockerCompose [Docker Compose]
    Nginx["nginx Alpine :80"]
    SpaDocker["SPA estatica"]
    ApiDocker["API .NET 9 :8080"]
    Sql["SQL Server 2019 :1433 ecommercedev"]
  end

  Jwt["JWT Bearer roles cliente / admin"]
  Swagger["Swagger / OpenAPI"]

  Browser --> SpaDev
  SpaDev -->|"HTTP CORS"| ApiDev
  ApiDev --> Jwt
  ApiDev --> Swagger
  ApiDev --> Sql

  Browser --> Nginx
  Nginx -->|"/"| SpaDocker
  Nginx -->|"/api/"| ApiDocker
  ApiDocker --> Jwt
  ApiDocker --> Swagger
  ApiDocker --> Sql
```

---

## Diagrama 2 — Capas internas

Un solo proyecto Web API con carpetas Clean Architecture. El frontend es standalone (sin NgModules), rutas lazy, signals y `localStorage`. El carrito no se persiste: las cantidades viven en el checkout.

```mermaid
flowchart TB
  subgraph angular [Angular 22 SPA]
    Features["Features: registro catalogo checkout estado-orden pendientes bitacora"]
    Guards["Guards: clienteGuard adminGuard"]
    Interceptors["Interceptors: auth error retry"]
    HttpSvc["HttpClient: AuthApi Cliente Orden Pago Inventario Log"]
    AuthState["AuthService signals + localStorage"]
  end

  subgraph api [API ASP.NET Core 9]
    MW["ExceptionHandlingMiddleware"]
    CtrlAuth["AuthController"]
    CtrlClientes["ClientesController"]
    CtrlProductos["ProductosController"]
    CtrlOrdenes["OrdenesController"]
    CtrlCompras["ComprasController"]
    CtrlLogs["LogsController"]
  end

  subgraph application [Application]
    Val["FluentValidation"]
    Sim["PaymentSimulator"]
    SvcCliente["ClienteService AuthService"]
    SvcProducto["ProductoService"]
    SvcOrden["OrdenService"]
    SvcPago["PagoService"]
    SvcInv["InventarioService"]
    SvcCompra["CompraService"]
    SvcLog["LogService"]
  end

  subgraph domain [Domain]
    Entities["Entidades: Cliente Producto Orden OrdenDetalle Pago LogEvento"]
    Contracts["IUnitOfWork IRepository IClienteRepository IProductoRepository IOrdenRepository IPagoRepository ILogRepository"]
  end

  subgraph infrastructure [Infrastructure]
    JwtSvc["JwtTokenService"]
    UoW["UnitOfWork"]
    Repos["Repositories"]
    EF["AppDbContext EF Core 9"]
  end

  subgraph db [SQL Server]
    Tables["Clientes Productos Ordenes OrdenDetalle Pagos Logs"]
  end

  Features --> Guards
  Features --> AuthState
  Features --> HttpSvc
  HttpSvc --> Interceptors
  Interceptors --> MW
  MW --> CtrlAuth
  MW --> CtrlClientes
  MW --> CtrlProductos
  MW --> CtrlOrdenes
  MW --> CtrlCompras
  MW --> CtrlLogs
  CtrlAuth --> SvcCliente
  CtrlClientes --> SvcCliente
  CtrlProductos --> SvcProducto
  CtrlOrdenes --> SvcOrden
  CtrlOrdenes --> SvcPago
  CtrlOrdenes --> SvcInv
  CtrlCompras --> SvcCompra
  CtrlLogs --> SvcLog
  SvcCompra --> SvcOrden
  SvcCompra --> SvcPago
  SvcCompra --> SvcInv
  SvcPago --> Sim
  SvcCliente --> Val
  SvcOrden --> Val
  SvcPago --> Val
  SvcCompra --> Val
  SvcCliente --> JwtSvc
  SvcCliente --> UoW
  SvcProducto --> UoW
  SvcOrden --> UoW
  SvcPago --> UoW
  SvcInv --> UoW
  SvcLog --> UoW
  UoW --> Repos
  Repos --> Contracts
  Repos --> EF
  EF --> Entities
  EF --> Tables
```

### Mapeo frontend → API

| Angular | Endpoint | Auth API |
|---------|----------|----------|
| `AuthApiService` | `POST /api/auth/identificar`, `POST /api/auth/admin` | Anonimo |
| `ClienteService` | `POST /api/clientes`, `GET /api/clientes/{id}` | Anonimo / Authorize |
| `InventarioService` | `GET /api/productos`, `POST /api/ordenes/{id}/inventario` | Anonimo / Admin |
| `OrdenService` | `POST /api/compras`, `GET/POST /api/ordenes...` | Cliente / Admin |
| `PagoService` | `POST /api/ordenes/{id}/pagos`, `.../pagos/reintentar` | Cliente |
| `LogService` | `GET/POST /api/logs` | Admin |

---

## Diagrama 3 — Modelo de datos (ER)

El modelo EF replica `db_script.sql`. No hay tabla de carrito, kardex ni catalogo de categorias: el stock vive en `Productos.Stock` y el historial en `Logs` (`INVENTARIO`).

- `OrdenDetalle.Subtotal` es columna calculada almacenada: `Cantidad * PrecioUnitario`.
- Unique `(OrdenId, ProductoId)` en `OrdenDetalle`.
- Delete: cascade `Ordenes` → `OrdenDetalle`; restrict `Productos` → `OrdenDetalle`, `Clientes` → `Ordenes`, `Ordenes` → `Pagos`.
- Estados orden: `pendiente`, `confirmada`, `cancelada`, `rechazada`.
- Estados pago: `autorizado`, `rechazado`, `pendiente`.
- Operaciones log: `INSERT`, `UPDATE`, `DELETE`, `ERROR`, `PAGO`, `INVENTARIO`.

```mermaid
erDiagram
  Clientes ||--o{ Ordenes : tiene
  Ordenes ||--o{ OrdenDetalle : contiene
  Productos ||--o{ OrdenDetalle : aparece_en
  Ordenes ||--o{ Pagos : registra

  Clientes {
    int ClienteId PK
    string Nombre
    string Apellido
    string Email
    string Telefono
    string Direccion
    datetime FechaRegistro
    bool Activo
  }

  Productos {
    int ProductoId PK
    string Nombre
    string Descripcion
    decimal Precio
    int Stock
    bool Activo
  }

  Ordenes {
    int OrdenId PK
    int ClienteId FK
    datetime FechaOrden
    string Estado
    decimal Total
    string Observaciones
  }

  OrdenDetalle {
    int OrdenDetalleId PK
    int OrdenId FK
    int ProductoId FK
    int Cantidad
    decimal PrecioUnitario
    decimal Subtotal
  }

  Pagos {
    int PagoId PK
    int OrdenId FK
    decimal Monto
    string Estado
    string MetodoPago
    string Referencia
    int Intentos
    datetime FechaPago
    datetime FechaRegistro
    string MensajeError
  }

  Logs {
    long LogId PK
    string TablaAfectada
    string Operacion
    string RegistroId
    string MensajeLog
    string Usuario
    datetime FechaEvento
  }
```

---

## Diagrama 4 — Secuencia de compra

`POST /api/compras` (policy `Cliente`) orquesta crear orden, simular pago (hasta 3 intentos) y descontar stock en una transaccion EF. `PaymentSimulator`: referencia que termina en `0000` → `rechazado`; monto mayor a `10000` → `pendiente` (revision admin); resto → `autorizado`.

```mermaid
sequenceDiagram
  participant FE as Angular Checkout
  participant API as CompraService
  participant Sim as PaymentSimulator
  participant DB as SQL Server

  FE->>API: POST /api/compras JWT cliente
  API->>API: BEGIN TRAN
  API->>DB: Crear orden pendiente y detalles
  loop Hasta 3 intentos o autorizado
    API->>Sim: Simular pago
    Sim-->>API: autorizado o rechazado o pendiente
    API->>DB: Insertar Pago
    alt pendiente
      API->>API: COMMIT orden pendiente
      API-->>FE: Pago pendiente de revision
    else rechazado y quedan intentos
      API->>DB: Orden sigue pendiente
    end
  end
  alt pago no autorizado
    API->>API: COMMIT orden rechazada
    API-->>FE: Pago rechazado
  else pago autorizado
    API->>DB: UPDATE Stock = Stock - n WHERE Stock mayor o igual n
    alt 1 fila actualizada
      API->>API: COMMIT orden confirmada
      API-->>FE: Compra exitosa
    else 0 filas
      API->>API: ROLLBACK InsufficientStock 409
      API-->>FE: Stock insuficiente
    end
  end
```

---

## Autenticacion

- Cliente: `POST /api/clientes` (registro) o `POST /api/auth/identificar` (email, sin password). JWT rol `cliente` y claim `clienteId`.
- Admin demo: `POST /api/auth/admin` con clave `demo-admin`. JWT rol `admin`.
- Frontend: token en `localStorage`; `authInterceptor` envia `Authorization: Bearer`; `errorInterceptor` hace logout en 401.
- Policies API: `Cliente` → `RequireRole("cliente")`; `Admin` → `RequireRole("admin")`.
