# Arquitectura

## Capas

```mermaid
flowchart LR
  subgraph angular [Angular22]
    UI[Componentes]
    Svc[Servicios HttpClient]
    Int[Interceptors]
  end
  subgraph api [API]
    Ctrl[Controllers]
    MW[ExceptionMiddleware]
  end
  subgraph app [Application]
    UseCases[Servicios de caso de uso]
    Val[FluentValidation]
  end
  subgraph infra [Infrastructure]
    UoW[UnitOfWork]
    Repos[Repositories]
    EF[AppDbContext]
  end
  subgraph db [SQLServer]
    Tables[Tablas script_master]
  end
  UI --> Svc --> Int --> Ctrl --> UseCases --> UoW --> Repos --> EF --> Tables
  Ctrl --> MW
  UseCases --> Val
```

## Flujo de compra y stock concurrente

```mermaid
sequenceDiagram
  participant FE as Angular
  participant API as CompraService
  participant DB as Productos
  FE->>API: POST /api/compras
  API->>API: BEGIN TRAN
  API->>API: Crear orden pendiente
  API->>API: Simular pago
  API->>DB: UPDATE Stock = Stock - n WHERE Stock >= n
  alt 1 fila
    API->>API: COMMIT confirmada
  else 0 filas
    API->>API: ROLLBACK InsufficientStock 409
  end
```

## Tablas

El modelo EF replica `script_master.sql`: `Clientes`, `Productos`, `Ordenes`, `OrdenDetalle`, `Pagos`, `Logs`.

No hay tabla de kardex ni catalogo separado: el stock vive en `Productos.Stock` y el historial en `Logs` (`INVENTARIO`).

## Autenticacion

- Cliente: registro o identificacion por email, JWT rol `cliente`.
- Admin demo: `POST /api/auth/admin` con clave `demo-admin`.
