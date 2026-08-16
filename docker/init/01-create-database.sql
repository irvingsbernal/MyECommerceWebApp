-- Opcional: crea la base si se usa script_master.sql en lugar de migraciones EF.
IF DB_ID(N'ecommercedev') IS NULL
BEGIN
    CREATE DATABASE ecommercedev;
END;
GO
