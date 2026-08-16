/* ============================================================
   E-COMMERCE - Script corregido v3
   SQL Server 2019 | Base de datos: ecommercedev
   ============================================================ */

USE ecommercedev;
GO

SET NOCOUNT ON;
GO

/* ============================================================
   PASO 0: Limpieza
   ============================================================ */
IF OBJECT_ID(N'dbo.sp_ProcesarCompraCompleta', N'P') IS NOT NULL DROP PROCEDURE dbo.sp_ProcesarCompraCompleta;
IF OBJECT_ID(N'dbo.sp_ConsultarEstadoOrden', N'P') IS NOT NULL DROP PROCEDURE dbo.sp_ConsultarEstadoOrden;
IF OBJECT_ID(N'dbo.sp_ActualizarInventario', N'P') IS NOT NULL DROP PROCEDURE dbo.sp_ActualizarInventario;
IF OBJECT_ID(N'dbo.sp_ProcesarPago', N'P') IS NOT NULL DROP PROCEDURE dbo.sp_ProcesarPago;
IF OBJECT_ID(N'dbo.sp_CrearOrden', N'P') IS NOT NULL DROP PROCEDURE dbo.sp_CrearOrden;
IF OBJECT_ID(N'dbo.sp_CrearCliente', N'P') IS NOT NULL DROP PROCEDURE dbo.sp_CrearCliente;
IF OBJECT_ID(N'dbo.sp_RegistrarLog', N'P') IS NOT NULL DROP PROCEDURE dbo.sp_RegistrarLog;
GO

IF OBJECT_ID(N'dbo.vw_EstadoOrden', N'V') IS NOT NULL DROP VIEW dbo.vw_EstadoOrden;
GO

IF OBJECT_ID(N'dbo.trg_Productos_ValidarStock', N'TR') IS NOT NULL DROP TRIGGER dbo.trg_Productos_ValidarStock;
IF OBJECT_ID(N'dbo.trg_Ordenes_AuditoriaEstado', N'TR') IS NOT NULL DROP TRIGGER dbo.trg_Ordenes_AuditoriaEstado;
IF OBJECT_ID(N'dbo.trg_Clientes_ValidarInsert', N'TR') IS NOT NULL DROP TRIGGER dbo.trg_Clientes_ValidarInsert;
GO

IF OBJECT_ID(N'dbo.Logs', N'U') IS NOT NULL DROP TABLE dbo.Logs;
IF OBJECT_ID(N'dbo.Pagos', N'U') IS NOT NULL DROP TABLE dbo.Pagos;
IF OBJECT_ID(N'dbo.OrdenDetalle', N'U') IS NOT NULL DROP TABLE dbo.OrdenDetalle;
IF OBJECT_ID(N'dbo.Ordenes', N'U') IS NOT NULL DROP TABLE dbo.Ordenes;
IF OBJECT_ID(N'dbo.Productos', N'U') IS NOT NULL DROP TABLE dbo.Productos;
IF OBJECT_ID(N'dbo.Clientes', N'U') IS NOT NULL DROP TABLE dbo.Clientes;
GO

IF TYPE_ID(N'dbo.TVP_OrdenDetalle') IS NOT NULL DROP TYPE dbo.TVP_OrdenDetalle;
GO

/* ============================================================
   PASO 1: Tablas
   ============================================================ */
CREATE TABLE dbo.Clientes (
    ClienteId     INT IDENTITY(1,1) NOT NULL,
    Nombre        NVARCHAR(100) NOT NULL,
    Apellido      NVARCHAR(100) NOT NULL,
    Email         NVARCHAR(255) NOT NULL,
    Telefono      NVARCHAR(20)  NULL,
    Direccion     NVARCHAR(300) NOT NULL,
    FechaRegistro DATETIME2(0)  NOT NULL CONSTRAINT DF_Clientes_FechaRegistro DEFAULT (SYSDATETIME()),
    Activo        BIT NOT NULL CONSTRAINT DF_Clientes_Activo DEFAULT (1),
    CONSTRAINT PK_Clientes PRIMARY KEY CLUSTERED (ClienteId),
    CONSTRAINT UQ_Clientes_Email UNIQUE (Email),
    CONSTRAINT CK_Clientes_Email CHECK (Email LIKE '%_@_%._%'),
    CONSTRAINT CK_Clientes_Nombre CHECK (LEN(LTRIM(RTRIM(Nombre))) > 0),
    CONSTRAINT CK_Clientes_Apellido CHECK (LEN(LTRIM(RTRIM(Apellido))) > 0)
);
GO

CREATE TABLE dbo.Productos (
    ProductoId  INT IDENTITY(1,1) NOT NULL,
    Nombre      NVARCHAR(200) NOT NULL,
    Descripcion NVARCHAR(500) NULL,
    Precio      DECIMAL(18,2) NOT NULL,
    Stock       INT NOT NULL,
    Activo      BIT NOT NULL CONSTRAINT DF_Productos_Activo DEFAULT (1),
    CONSTRAINT PK_Productos PRIMARY KEY CLUSTERED (ProductoId),
    CONSTRAINT CK_Productos_Precio CHECK (Precio > 0),
    CONSTRAINT CK_Productos_Stock CHECK (Stock >= 0)
);
GO

CREATE TABLE dbo.Ordenes (
    OrdenId       INT IDENTITY(1,1) NOT NULL,
    ClienteId     INT NOT NULL,
    FechaOrden    DATETIME2(0) NOT NULL CONSTRAINT DF_Ordenes_FechaOrden DEFAULT (SYSDATETIME()),
    Estado        NVARCHAR(20) NOT NULL,
    Total         DECIMAL(18,2) NOT NULL,
    Observaciones NVARCHAR(500) NULL,
    CONSTRAINT PK_Ordenes PRIMARY KEY CLUSTERED (OrdenId),
    CONSTRAINT FK_Ordenes_Clientes FOREIGN KEY (ClienteId) REFERENCES dbo.Clientes (ClienteId),
    CONSTRAINT CK_Ordenes_Estado CHECK (Estado IN ('pendiente', 'confirmada', 'cancelada', 'rechazada')),
    CONSTRAINT CK_Ordenes_Total CHECK (Total >= 0)
);
GO

CREATE TABLE dbo.OrdenDetalle (
    OrdenDetalleId INT IDENTITY(1,1) NOT NULL,
    OrdenId        INT NOT NULL,
    ProductoId     INT NOT NULL,
    Cantidad       INT NOT NULL,
    PrecioUnitario DECIMAL(18,2) NOT NULL,
    Subtotal       AS (Cantidad * PrecioUnitario) PERSISTED,
    CONSTRAINT PK_OrdenDetalle PRIMARY KEY CLUSTERED (OrdenDetalleId),
    CONSTRAINT FK_OrdenDetalle_Ordenes FOREIGN KEY (OrdenId) REFERENCES dbo.Ordenes (OrdenId) ON DELETE CASCADE,
    CONSTRAINT FK_OrdenDetalle_Productos FOREIGN KEY (ProductoId) REFERENCES dbo.Productos (ProductoId),
    CONSTRAINT CK_OrdenDetalle_Cantidad CHECK (Cantidad > 0),
    CONSTRAINT CK_OrdenDetalle_PrecioUnitario CHECK (PrecioUnitario > 0),
    CONSTRAINT UQ_OrdenDetalle_Orden_Producto UNIQUE (OrdenId, ProductoId)
);
GO

CREATE TABLE dbo.Pagos (
    PagoId        INT IDENTITY(1,1) NOT NULL,
    OrdenId       INT NOT NULL,
    Monto         DECIMAL(18,2) NOT NULL,
    Estado        NVARCHAR(20) NOT NULL,
    MetodoPago    NVARCHAR(50) NOT NULL,
    Referencia    NVARCHAR(100) NULL,
    Intentos      INT NOT NULL CONSTRAINT DF_Pagos_Intentos DEFAULT (0),
    FechaPago     DATETIME2(0) NULL,
    FechaRegistro DATETIME2(0) NOT NULL CONSTRAINT DF_Pagos_FechaRegistro DEFAULT (SYSDATETIME()),
    MensajeError  NVARCHAR(500) NULL,
    CONSTRAINT PK_Pagos PRIMARY KEY CLUSTERED (PagoId),
    CONSTRAINT FK_Pagos_Ordenes FOREIGN KEY (OrdenId) REFERENCES dbo.Ordenes (OrdenId),
    CONSTRAINT CK_Pagos_Estado CHECK (Estado IN ('autorizado', 'rechazado', 'pendiente')),
    CONSTRAINT CK_Pagos_Monto CHECK (Monto > 0),
    CONSTRAINT CK_Pagos_Intentos CHECK (Intentos >= 0)
);
GO

CREATE TABLE dbo.Logs (
    LogId         BIGINT IDENTITY(1,1) NOT NULL,
    TablaAfectada NVARCHAR(100) NOT NULL,
    Operacion     NVARCHAR(20) NOT NULL,
    RegistroId    NVARCHAR(50) NULL,
    MensajeLog    NVARCHAR(MAX) NULL,
    Usuario       NVARCHAR(128) NOT NULL CONSTRAINT DF_Logs_Usuario DEFAULT (SUSER_SNAME()),
    FechaEvento   DATETIME2(0) NOT NULL CONSTRAINT DF_Logs_FechaEvento DEFAULT (SYSDATETIME()),
    CONSTRAINT PK_Logs PRIMARY KEY CLUSTERED (LogId),
    CONSTRAINT CK_Logs_Operacion CHECK (Operacion IN ('INSERT', 'UPDATE', 'DELETE', 'ERROR', 'PAGO', 'INVENTARIO'))
);
GO

/* ============================================================
   PASO 2: Indices
   ============================================================ */
CREATE NONCLUSTERED INDEX IX_Clientes_Email ON dbo.Clientes (Email);
CREATE NONCLUSTERED INDEX IX_Ordenes_Estado ON dbo.Ordenes (Estado) INCLUDE (ClienteId, FechaOrden, Total);
CREATE NONCLUSTERED INDEX IX_Ordenes_ClienteId ON dbo.Ordenes (ClienteId, FechaOrden DESC);
CREATE NONCLUSTERED INDEX IX_Pagos_Estado ON dbo.Pagos (Estado) INCLUDE (OrdenId, Monto, FechaPago);
CREATE NONCLUSTERED INDEX IX_Logs_FechaEvento ON dbo.Logs (FechaEvento DESC);
GO

/* ============================================================
   PASO 3: Tipo de tabla (TVP)
   ============================================================ */
CREATE TYPE dbo.TVP_OrdenDetalle AS TABLE (
    ProductoId INT NOT NULL,
    Cantidad   INT NOT NULL
);
GO

/* ============================================================
   PASO 4: Procedimiento auxiliar de bitacora
   NOTA: parametro renombrado a @MensajeLog (evita conflicto con TVP)
   ============================================================ */
CREATE PROCEDURE dbo.sp_RegistrarLog
    @TablaAfectada NVARCHAR(100),
    @Operacion     NVARCHAR(20),
    @RegistroId    NVARCHAR(50) = NULL,
    @MensajeLog    NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.Logs (TablaAfectada, Operacion, RegistroId, MensajeLog)
    VALUES (@TablaAfectada, @Operacion, @RegistroId, @MensajeLog);
END;
GO

/* ============================================================
   PASO 5: Triggers
   ============================================================ */
CREATE TRIGGER dbo.trg_Clientes_ValidarInsert
ON dbo.Clientes
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (
        SELECT 1 FROM inserted
        WHERE Nombre IS NULL OR Apellido IS NULL OR Email IS NULL OR Direccion IS NULL
    )
    BEGIN
        RAISERROR('Error: Nombre, Apellido, Email y Direccion son obligatorios.', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END;

    INSERT INTO dbo.Logs (TablaAfectada, Operacion, RegistroId, MensajeLog)
    SELECT 'Clientes', 'INSERT', CAST(ClienteId AS NVARCHAR(50)), N'Cliente registrado: ' + Email
    FROM inserted;
END;
GO

CREATE TRIGGER dbo.trg_Ordenes_AuditoriaEstado
ON dbo.Ordenes
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF UPDATE(Estado)
    BEGIN
        INSERT INTO dbo.Logs (TablaAfectada, Operacion, RegistroId, MensajeLog)
        SELECT 'Ordenes', 'UPDATE', CAST(i.OrdenId AS NVARCHAR(50)),
               N'Estado cambio de ' + d.Estado + N' a ' + i.Estado
        FROM inserted i
        INNER JOIN deleted d ON i.OrdenId = d.OrdenId
        WHERE i.Estado <> d.Estado;
    END;
END;
GO

CREATE TRIGGER dbo.trg_Productos_ValidarStock
ON dbo.Productos
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM inserted WHERE Stock < 0)
    BEGIN
        RAISERROR('Error: El stock no puede ser negativo.', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END;
END;
GO

/* ============================================================
   PASO 6: Vista
   ============================================================ */
CREATE VIEW dbo.vw_EstadoOrden
AS
SELECT
    o.OrdenId,
    o.FechaOrden,
    o.Estado AS EstadoOrden,
    o.Total,
    c.ClienteId,
    c.Nombre + N' ' + c.Apellido AS ClienteNombre,
    c.Email,
    p.PagoId,
    p.Estado AS EstadoPago,
    p.MetodoPago,
    p.Intentos AS IntentosPago,
    p.FechaPago,
    p.MensajeError,
    (SELECT COUNT(*) FROM dbo.OrdenDetalle od WHERE od.OrdenId = o.OrdenId) AS TotalProductos
FROM dbo.Ordenes o
INNER JOIN dbo.Clientes c ON o.ClienteId = c.ClienteId
LEFT JOIN dbo.Pagos p ON p.OrdenId = o.OrdenId
    AND p.PagoId = (SELECT MAX(PagoId) FROM dbo.Pagos px WHERE px.OrdenId = o.OrdenId);
GO

/* ============================================================
   PASO 7: Procedimientos almacenados
   ============================================================ */

CREATE PROCEDURE dbo.sp_CrearCliente
    @Nombre    NVARCHAR(100),
    @Apellido  NVARCHAR(100),
    @Email     NVARCHAR(255),
    @Telefono  NVARCHAR(20)  = NULL,
    @Direccion NVARCHAR(300),
    @ClienteId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @ErrMsg NVARCHAR(4000);
    DECLARE @ErrSev INT;
    DECLARE @ErrSt  INT;

    BEGIN TRY
        BEGIN TRANSACTION;

        IF @Nombre IS NULL OR LEN(LTRIM(RTRIM(@Nombre))) = 0
            RAISERROR('El nombre es obligatorio.', 16, 1);

        IF @Apellido IS NULL OR LEN(LTRIM(RTRIM(@Apellido))) = 0
            RAISERROR('El apellido es obligatorio.', 16, 1);

        IF @Email IS NULL OR @Email NOT LIKE '%_@_%._%'
            RAISERROR('El email es obligatorio y debe tener formato valido.', 16, 1);

        IF @Direccion IS NULL OR LEN(LTRIM(RTRIM(@Direccion))) = 0
            RAISERROR('La direccion es obligatoria.', 16, 1);

        IF EXISTS (SELECT 1 FROM dbo.Clientes WHERE Email = @Email)
            RAISERROR('Ya existe un cliente con ese email.', 16, 1);

        INSERT INTO dbo.Clientes (Nombre, Apellido, Email, Telefono, Direccion)
        VALUES (@Nombre, @Apellido, @Email, @Telefono, @Direccion);

        SET @ClienteId = SCOPE_IDENTITY();
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;

        SET @ErrMsg = ERROR_MESSAGE();
        EXEC dbo.sp_RegistrarLog N'Clientes', N'ERROR', NULL, @ErrMsg;

        SET @ErrSev = ERROR_SEVERITY();
        SET @ErrSt  = ERROR_STATE();
        RAISERROR(@ErrMsg, @ErrSev, @ErrSt);
    END CATCH;
END;
GO

CREATE PROCEDURE dbo.sp_CrearOrden
    @ClienteId   INT,
    @LineasOrden dbo.TVP_OrdenDetalle READONLY,
    @OrdenId     INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @Total      DECIMAL(18,2) = 0;
    DECLARE @OrdenIdStr NVARCHAR(50);
    DECLARE @MsgLog     NVARCHAR(500);
    DECLARE @ErrMsg     NVARCHAR(4000);
    DECLARE @ErrSev     INT;
    DECLARE @ErrSt      INT;

    BEGIN TRY
        BEGIN TRANSACTION;

        IF NOT EXISTS (SELECT 1 FROM dbo.Clientes WHERE ClienteId = @ClienteId AND Activo = 1)
            RAISERROR('Cliente no existe o esta inactivo.', 16, 1);

        IF NOT EXISTS (SELECT 1 FROM @LineasOrden)
            RAISERROR('La orden debe tener al menos un producto.', 16, 1);

        IF EXISTS (
            SELECT 1
            FROM @LineasOrden lo
            LEFT JOIN dbo.Productos p ON lo.ProductoId = p.ProductoId
            WHERE p.ProductoId IS NULL OR p.Activo = 0
        )
            RAISERROR('Uno o mas productos no existen o estan inactivos.', 16, 1);

        IF EXISTS (
            SELECT 1
            FROM @LineasOrden lo
            INNER JOIN dbo.Productos p ON lo.ProductoId = p.ProductoId
            WHERE lo.Cantidad > p.Stock
        )
            RAISERROR('Stock insuficiente para uno o mas productos.', 16, 1);

        SELECT @Total = SUM(lo.Cantidad * p.Precio)
        FROM @LineasOrden lo
        INNER JOIN dbo.Productos p ON lo.ProductoId = p.ProductoId;

        INSERT INTO dbo.Ordenes (ClienteId, Estado, Total)
        VALUES (@ClienteId, N'pendiente', @Total);

        SET @OrdenId = SCOPE_IDENTITY();

        INSERT INTO dbo.OrdenDetalle (OrdenId, ProductoId, Cantidad, PrecioUnitario)
        SELECT @OrdenId, lo.ProductoId, lo.Cantidad, p.Precio
        FROM @LineasOrden lo
        INNER JOIN dbo.Productos p ON lo.ProductoId = p.ProductoId;

        SET @OrdenIdStr = CAST(@OrdenId AS NVARCHAR(50));
        SET @MsgLog = N'Orden creada. Total: ' + CAST(@Total AS NVARCHAR(50));
        EXEC dbo.sp_RegistrarLog N'Ordenes', N'INSERT', @OrdenIdStr, @MsgLog;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;

        SET @ErrMsg = ERROR_MESSAGE();
        EXEC dbo.sp_RegistrarLog N'Ordenes', N'ERROR', NULL, @ErrMsg;

        SET @ErrSev = ERROR_SEVERITY();
        SET @ErrSt  = ERROR_STATE();
        RAISERROR(@ErrMsg, @ErrSev, @ErrSt);
    END CATCH;
END;
GO

CREATE PROCEDURE dbo.sp_ProcesarPago
    @OrdenId      INT,
    @Monto        DECIMAL(18,2),
    @MetodoPago   NVARCHAR(50),
    @Referencia   NVARCHAR(100) = NULL,
    @ForzarEstado NVARCHAR(20)  = NULL,
    @PagoId       INT OUTPUT,
    @EstadoPago   NVARCHAR(20) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @IntentosActuales INT;
    DECLARE @MaxIntentos      INT = 3;
    DECLARE @PagoIdStr        NVARCHAR(50);
    DECLARE @MsgLog           NVARCHAR(500);
    DECLARE @ErrMsg           NVARCHAR(4000);
    DECLARE @ErrSev           INT;
    DECLARE @ErrSt            INT;

    BEGIN TRY
        BEGIN TRANSACTION;

        IF NOT EXISTS (SELECT 1 FROM dbo.Ordenes WHERE OrdenId = @OrdenId)
            RAISERROR('La orden no existe.', 16, 1);

        IF NOT EXISTS (SELECT 1 FROM dbo.Ordenes WHERE OrdenId = @OrdenId AND Estado = N'pendiente')
            RAISERROR('Solo se pueden procesar pagos de ordenes pendientes.', 16, 1);

        IF @Monto <> (SELECT Total FROM dbo.Ordenes WHERE OrdenId = @OrdenId)
            RAISERROR('El monto del pago no coincide con el total de la orden.', 16, 1);

        SELECT @IntentosActuales = ISNULL(MAX(Intentos), 0)
        FROM dbo.Pagos WHERE OrdenId = @OrdenId;

        IF @IntentosActuales >= @MaxIntentos
            RAISERROR('Se alcanzo el maximo de reintentos de pago.', 16, 1);

        SET @IntentosActuales = @IntentosActuales + 1;

        IF @ForzarEstado IS NOT NULL
            SET @EstadoPago = @ForzarEstado;
        ELSE IF @Referencia LIKE N'%0000'
            SET @EstadoPago = N'rechazado';
        ELSE IF @Monto > 10000
            SET @EstadoPago = N'pendiente';
        ELSE
            SET @EstadoPago = N'autorizado';

        INSERT INTO dbo.Pagos (OrdenId, Monto, Estado, MetodoPago, Referencia, Intentos, FechaPago, MensajeError)
        VALUES (
            @OrdenId, @Monto, @EstadoPago, @MetodoPago, @Referencia, @IntentosActuales,
            CASE WHEN @EstadoPago = N'autorizado' THEN SYSDATETIME() ELSE NULL END,
            CASE WHEN @EstadoPago = N'rechazado' THEN N'Pago rechazado por el procesador.' ELSE NULL END
        );

        SET @PagoId = SCOPE_IDENTITY();

        IF @EstadoPago = N'autorizado'
            UPDATE dbo.Ordenes SET Estado = N'confirmada' WHERE OrdenId = @OrdenId;
        ELSE IF @EstadoPago = N'rechazado'
            UPDATE dbo.Ordenes SET Estado = N'rechazada' WHERE OrdenId = @OrdenId;

        SET @PagoIdStr = CAST(@PagoId AS NVARCHAR(50));
        SET @MsgLog = N'Pago ' + @EstadoPago + N'. Intento #' + CAST(@IntentosActuales AS NVARCHAR(10));
        EXEC dbo.sp_RegistrarLog N'Pagos', N'PAGO', @PagoIdStr, @MsgLog;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;

        SET @ErrMsg = ERROR_MESSAGE();
        EXEC dbo.sp_RegistrarLog N'Pagos', N'ERROR', NULL, @ErrMsg;

        SET @ErrSev = ERROR_SEVERITY();
        SET @ErrSt  = ERROR_STATE();
        RAISERROR(@ErrMsg, @ErrSev, @ErrSt);
    END CATCH;
END;
GO

CREATE PROCEDURE dbo.sp_ActualizarInventario
    @OrdenId INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @OrdenIdStr NVARCHAR(50);
    DECLARE @ErrMsg     NVARCHAR(4000);
    DECLARE @ErrSev     INT;
    DECLARE @ErrSt      INT;

    BEGIN TRY
        BEGIN TRANSACTION;

        IF NOT EXISTS (SELECT 1 FROM dbo.Ordenes WHERE OrdenId = @OrdenId AND Estado = N'confirmada')
            RAISERROR('Solo se puede actualizar inventario de ordenes confirmadas.', 16, 1);

        IF EXISTS (
            SELECT 1
            FROM dbo.OrdenDetalle od
            INNER JOIN dbo.Productos p ON od.ProductoId = p.ProductoId
            WHERE od.OrdenId = @OrdenId AND od.Cantidad > p.Stock
        )
            RAISERROR('Stock insuficiente al confirmar la venta.', 16, 1);

        UPDATE p
        SET p.Stock = p.Stock - od.Cantidad
        FROM dbo.Productos p
        INNER JOIN dbo.OrdenDetalle od ON p.ProductoId = od.ProductoId
        WHERE od.OrdenId = @OrdenId;

        SET @OrdenIdStr = CAST(@OrdenId AS NVARCHAR(50));
        EXEC dbo.sp_RegistrarLog N'Productos', N'INVENTARIO', @OrdenIdStr, N'Inventario actualizado por venta confirmada.';

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;

        SET @ErrMsg = ERROR_MESSAGE();
        EXEC dbo.sp_RegistrarLog N'Productos', N'ERROR', NULL, @ErrMsg;

        SET @ErrSev = ERROR_SEVERITY();
        SET @ErrSt  = ERROR_STATE();
        RAISERROR(@ErrMsg, @ErrSev, @ErrSt);
    END CATCH;
END;
GO

CREATE PROCEDURE dbo.sp_ProcesarCompraCompleta
    @ClienteId    INT,
    @LineasOrden  dbo.TVP_OrdenDetalle READONLY,
    @MetodoPago   NVARCHAR(50),
    @Referencia   NVARCHAR(100) = NULL,
    @OrdenId      INT OUTPUT,
    @Resultado    NVARCHAR(500) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @OrdenIdLocal  INT;
    DECLARE @PagoId        INT;
    DECLARE @EstadoPago    NVARCHAR(20);
    DECLARE @Monto         DECIMAL(18,2);
    DECLARE @Intento       INT = 0;
    DECLARE @MaxReintentos INT = 3;
    DECLARE @PagoExitoso   BIT = 0;
    DECLARE @OrdenIdStr    NVARCHAR(50);
    DECLARE @MsgLog        NVARCHAR(500);
    DECLARE @ErrMsg        NVARCHAR(4000);
    DECLARE @ErrSev        INT;
    DECLARE @ErrSt         INT;

    BEGIN TRY
        EXEC dbo.sp_CrearOrden @ClienteId, @LineasOrden, @OrdenIdLocal OUTPUT;

        SET @OrdenId = @OrdenIdLocal;

        SELECT @Monto = Total FROM dbo.Ordenes WHERE OrdenId = @OrdenIdLocal;

        WHILE @Intento < @MaxReintentos AND @PagoExitoso = 0
        BEGIN
            SET @Intento = @Intento + 1;

            BEGIN TRY
                EXEC dbo.sp_ProcesarPago
                    @OrdenId    = @OrdenIdLocal,
                    @Monto      = @Monto,
                    @MetodoPago = @MetodoPago,
                    @Referencia = @Referencia,
                    @PagoId     = @PagoId OUTPUT,
                    @EstadoPago = @EstadoPago OUTPUT;

                IF @EstadoPago = N'autorizado'
                    SET @PagoExitoso = 1;
                ELSE IF @EstadoPago = N'pendiente'
                BEGIN
                    SET @Resultado = N'Pago pendiente de revision.';
                    RETURN;
                END
                ELSE IF @Intento < @MaxReintentos
                    UPDATE dbo.Ordenes SET Estado = N'pendiente' WHERE OrdenId = @OrdenIdLocal;
            END TRY
            BEGIN CATCH
                SET @ErrMsg = ERROR_MESSAGE();

                IF @Intento >= @MaxReintentos
                BEGIN
                    SET @ErrSev = ERROR_SEVERITY();
                    SET @ErrSt  = ERROR_STATE();
                    RAISERROR(@ErrMsg, @ErrSev, @ErrSt);
                END;

                SET @OrdenIdStr = CAST(@OrdenIdLocal AS NVARCHAR(50));
                SET @MsgLog = N'Reintento ' + CAST(@Intento AS NVARCHAR(10)) + N': ' + @ErrMsg;
                EXEC dbo.sp_RegistrarLog N'Pagos', N'ERROR', @OrdenIdStr, @MsgLog;
            END CATCH;
        END;

        IF @PagoExitoso = 0
        BEGIN
            SET @Resultado = N'Pago rechazado despues de todos los reintentos.';
            RETURN;
        END;

        EXEC dbo.sp_ActualizarInventario @OrdenId = @OrdenIdLocal;

        SET @Resultado = N'Compra exitosa. Orden #' + CAST(@OrdenIdLocal AS NVARCHAR(10)) + N' confirmada.';
    END TRY
    BEGIN CATCH
        SET @Resultado = ERROR_MESSAGE();
        SET @OrdenIdStr = CAST(@OrdenIdLocal AS NVARCHAR(50));
        EXEC dbo.sp_RegistrarLog N'Ordenes', N'ERROR', @OrdenIdStr, @Resultado;

        SET @ErrMsg = ERROR_MESSAGE();
        SET @ErrSev = ERROR_SEVERITY();
        SET @ErrSt  = ERROR_STATE();
        RAISERROR(@ErrMsg, @ErrSev, @ErrSt);
    END CATCH;
END;
GO

CREATE PROCEDURE dbo.sp_ConsultarEstadoOrden
    @OrdenId INT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.Ordenes WHERE OrdenId = @OrdenId)
        RAISERROR('Orden no encontrada.', 16, 1);

    SELECT * FROM dbo.vw_EstadoOrden WHERE OrdenId = @OrdenId;

    SELECT od.OrdenDetalleId, p.ProductoId, p.Nombre AS Producto,
           od.Cantidad, od.PrecioUnitario, od.Subtotal, p.Stock AS StockActual
    FROM dbo.OrdenDetalle od
    INNER JOIN dbo.Productos p ON od.ProductoId = p.ProductoId
    WHERE od.OrdenId = @OrdenId;

    SELECT PagoId, Monto, Estado, MetodoPago, Referencia,
           Intentos, FechaPago, FechaRegistro, MensajeError
    FROM dbo.Pagos
    WHERE OrdenId = @OrdenId
    ORDER BY PagoId;
END;
GO

/* ============================================================
   PASO 8: Datos de prueba
   ============================================================ */
INSERT INTO dbo.Productos (Nombre, Descripcion, Precio, Stock)
VALUES
    (N'Laptop Pro 15"',    N'Laptop 16GB RAM, 512GB SSD', 1299.99, 10),
    (N'Mouse inalambrico', N'Mouse ergonomico',            29.99,  50),
    (N'Teclado mecanico',  N'Teclado RGB',                 89.99,  30);
GO

DECLARE @NuevoClienteId INT;
EXEC dbo.sp_CrearCliente
    @Nombre = N'Juan', @Apellido = N'Perez',
    @Email = N'juan.perez@email.com', @Telefono = N'555-1234',
    @Direccion = N'Av. Reforma 123, CDMX', @ClienteId = @NuevoClienteId OUTPUT;
SELECT @NuevoClienteId AS ClienteRegistrado;
GO

DECLARE @ClienteId INT = 1;
DECLARE @OrdenId   INT;
DECLARE @Resultado NVARCHAR(500);
DECLARE @Lineas    dbo.TVP_OrdenDetalle;

INSERT INTO @Lineas (ProductoId, Cantidad) VALUES (1, 1), (2, 2);

EXEC dbo.sp_ProcesarCompraCompleta
    @ClienteId = @ClienteId, @LineasOrden = @Lineas,
    @MetodoPago = N'Tarjeta de credito', @Referencia = N'VISA-4532',
    @OrdenId = @OrdenId OUTPUT, @Resultado = @Resultado OUTPUT;

SELECT @OrdenId AS OrdenCreada, @Resultado AS Mensaje;
GO

DECLARE @OrdenId2   INT;
DECLARE @Resultado2 NVARCHAR(500);
DECLARE @Lineas2    dbo.TVP_OrdenDetalle;

INSERT INTO @Lineas2 (ProductoId, Cantidad) VALUES (3, 1);

EXEC dbo.sp_ProcesarCompraCompleta
    @ClienteId = 1, @LineasOrden = @Lineas2,
    @MetodoPago = N'Tarjeta de debito', @Referencia = N'CARD-0000',
    @OrdenId = @OrdenId2 OUTPUT, @Resultado = @Resultado2 OUTPUT;

SELECT @OrdenId2 AS OrdenRechazada, @Resultado2 AS Mensaje;
GO

EXEC dbo.sp_ConsultarEstadoOrden @OrdenId = 1;
GO

SELECT TOP 20 LogId, TablaAfectada, Operacion, RegistroId, MensajeLog, Usuario, FechaEvento
FROM dbo.Logs ORDER BY FechaEvento DESC;
GO

SELECT OrdenId, ClienteNombre, Email, EstadoOrden, EstadoPago, Total, FechaOrden
FROM dbo.vw_EstadoOrden ORDER BY FechaOrden DESC;
GO

PRINT 'Script ejecutado correctamente.';
GO