-- Crea la base de datos si el contenedor de SQL Server arranca por primera vez.
IF DB_ID(N'ecommercedev') IS NULL
BEGIN
    CREATE DATABASE ecommercedev;
END;
GO
