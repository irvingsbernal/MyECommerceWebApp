# Registro de uso de IA

## Generado

- Dominio alineado a `db_script.sql` (entidades, constantes, contratos de repositorio y UoW).
- `TryDecrementStockAsync` con `ExecuteUpdateAsync` (`WHERE Stock >= cantidad`).
- Casos de uso: cliente, orden, pago simulado, inventario, compra completa con transaccion y rollback, bitacora.
- API REST, middleware ProblemDetails, JWT, Swagger, CORS.
- Proyecto xUnit (reglas de pago e integracion de carrera de stock).
- Frontend Angular 22: servicios, interceptores, guards, registro, catalogo, checkout, estado de orden y bitacora.
- Docker Compose, Postman, README y diagrama de arquitectura.

## Reutilizado de la base compilada

- Solucion y proyecto API .NET 9 con carpetas Clean Architecture.
- Patron `IRepository<T>` / `IUnitOfWork` / `Repository<T>` / `UnitOfWork` (se extendio, no se reescribio desde cero).
- EF Core + SQL Server, Swashbuckle (se cableo), Angular CLI 22 con SSR.

## Modificado

- `IRepository<T>` ya no exige `BaseEntity.Id`; usa `FindAsync`.
- `AppDbContext` mapea tablas en espanol del script.
- `Program.cs`: Swagger, CORS, JWT, migraciones y seed.
- Frontend scaffold reemplazado por el flujo de compra.

## Descartado

- Entidad `Category` y CRUD asociado (no existe en `script_master.sql`).
- `BaseEntity` con `CreatedAt`/`UpdatedAt` (el script usa otras columnas de auditoria).
- Llamar stored procedures como camino principal (la rubrica pide UoW/Repository; la logica se reimplemento en Application).
- Tablas extra de inventario/kardex y catalogo separado.
- Semilla de ordenes/pagos (solo productos y clientes).
- `Microsoft.AspNetCore.OpenApi` junto a Swashbuckle 10 (conflicto de ensamblados Microsoft.OpenApi 1.x vs 2.x).
