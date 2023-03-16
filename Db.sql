-- Create a new database called 'Parkhausverwaltung'
-- Connect to the 'master' database to run this snippet
USE master
GO
-- Create the new database if it does not exist already
IF NOT EXISTS (
    SELECT name
        FROM sys.databases
        WHERE name = N'Parkhausverwaltung'
)
CREATE DATABASE Parkhausverwaltung
GO


USE Parkhausverwaltung
GO

-- Create a new table called 'Parkhaus' in schema 'SchemaName'
-- Drop the table if it already exists
IF OBJECT_ID('Parkhaus', 'U') IS NOT NULL
DROP TABLE Parkhaus
GO
-- Create the table in the specified schema
CREATE TABLE Parkhaus
(
    ParkhausId INT IDENTITY(10000,1) NOT NULL PRIMARY KEY, -- primary key column
    Name [NVARCHAR](50) NOT NULL,
    DayPrice [INT] NOT NULL,
    DefaultPrice [INT] NOT NULL,
    MonthlyPrice [INT] NOT NULL
);
GO

-- Create a new table called 'Mieter' in schema 'SchemaName'
-- Drop the table if it already exists
IF OBJECT_ID('Mieter', 'U') IS NOT NULL
DROP TABLE Mieter
GO
-- Create the table in the specified schema
CREATE TABLE Mieter
(
    MieterId INT IDENTITY(10000,1) NOT NULL PRIMARY KEY, -- primary key column
    Name [NVARCHAR](50) NOT NULL,
    MieterCode INT NOT NULL UNIQUE,
    ParkhausId INT NOT NULL FOREIGN KEY REFERENCES Parkhaus(ParkhausId),
    SlotNr INT NOT NULL,
    StartDate DATETIME NOT NULL,
    EndDate DATETIME,
    PaymentOpen BIT NOT NULL
);
GO

-- Create a new table called 'Tarif' in schema 'SchemaName'
-- Drop the table if it already exists
IF OBJECT_ID('Tarif', 'U') IS NOT NULL
DROP TABLE Tarif
GO
-- Create the table in the specified schema
CREATE TABLE Tarif
(
    TarifId INT IDENTITY(10000,1) NOT NULL PRIMARY KEY, -- primary key column
    ParkhausId INT NOT NULL FOREIGN KEY REFERENCES Parkhaus(ParkhausId),
    Preis DECIMAL(10, 1) NOT NULL,
    StartTime TIME NOT NULL,
    EndTime TIME NOT NULL,
    StartDate DATETIME NOT NULL,
    EndDate DATETIME,
    WorkDay BIT NOT NULL
);
GO

-- Create a new table called 'FLoor' in schema 'SchemaName'
-- Drop the table if it already exists
IF OBJECT_ID('FLoor', 'U') IS NOT NULL
DROP TABLE FLoor
GO
-- Create the table in the specified schema
CREATE TABLE FLoor
(
    FLoorId INT IDENTITY(10000,1) NOT NULL PRIMARY KEY, -- primary key column
    ParkhausId INT NOT NULL FOREIGN KEY REFERENCES Parkhaus(ParkhausId),
    FloorNr INT NOT NULL,
    SlotCount INT NOT NULL
);
GO

-- Create a new table called 'Visit' in schema 'SchemaName'
-- Drop the table if it already exists
IF OBJECT_ID('Visit', 'U') IS NOT NULL
DROP TABLE Visit
GO
-- Create the table in the specified schema
CREATE TABLE Visit
(
    VisitId INT IDENTITY(10000,1) NOT NULL PRIMARY KEY, -- primary key column
    ParkhausId INT NOT NULL FOREIGN KEY REFERENCES Parkhaus(ParkhausId),
    Arrival DATETIME NOT NULL,
    Departure DATETIME,
    Cost DECIMAL(10,1) NOT NULL,
    TicketNr VARCHAR(50),
    MieterId INT FOREIGN KEY REFERENCES Mieter(MieterId),
    SlotNr INT,
    HasLeft BIT
);
GO
