/* ============================================================
   Vehicle Management System - database script (SQL Server)
   Creates the database, the Vehicles table and sample data.
   Safe to run more than once (idempotent).
   ============================================================ */

IF DB_ID(N'VehicleManagementDB') IS NULL
BEGIN
    CREATE DATABASE VehicleManagementDB;
END
GO

USE VehicleManagementDB;
GO

IF OBJECT_ID(N'dbo.Vehicles', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Vehicles
    (
        Id              INT IDENTITY(1,1) NOT NULL,
        Make            NVARCHAR(50)      NOT NULL,   -- e.g. Toyota
        Model           NVARCHAR(50)      NOT NULL,   -- e.g. Corolla
        LicensePlate    NVARCHAR(10)      NOT NULL,   -- e.g. ABC1D23
        ManufactureYear INT               NOT NULL,   -- e.g. 2020

        CONSTRAINT PK_Vehicles PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT UQ_Vehicles_LicensePlate UNIQUE (LicensePlate),
        CONSTRAINT CK_Vehicles_ManufactureYear CHECK (ManufactureYear BETWEEN 1900 AND 2100)
    );
END
GO

/* Optional sample data (only inserted when the table is empty) */
IF NOT EXISTS (SELECT 1 FROM dbo.Vehicles)
BEGIN
    INSERT INTO dbo.Vehicles (Make, Model, LicensePlate, ManufactureYear) VALUES
        (N'Toyota',     N'Corolla', N'ABC1D23', 2020),
        (N'Volkswagen', N'Gol',     N'XYZ9K88', 2018),
        (N'Honda',      N'Civic',   N'QWE4R56', 2022);
END
GO

SELECT * FROM dbo.Vehicles;
GO
