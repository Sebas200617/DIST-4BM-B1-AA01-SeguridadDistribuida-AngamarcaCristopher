-- Crear la base de datos
CREATE DATABASE UsuariosDB;
GO

USE UsuariosDB;
GO

-- Perfiles de usuario 
CREATE TABLE dbo.Perfiles
(
    Id               INT IDENTITY(1,1) NOT NULL,
    UsuarioId        NVARCHAR(450)     NOT NULL,  
    Nombre           NVARCHAR(100)     NOT NULL,
    Apellido         NVARCHAR(100)     NULL,
    Correo           NVARCHAR(256)     NOT NULL,
    Telefono         NVARCHAR(20)      NULL,
    Direccion        NVARCHAR(250)     NULL,
    Ciudad           NVARCHAR(100)     NULL,
    FechaCreacion    DATETIME2         NOT NULL CONSTRAINT DF_Perfiles_FechaCreacion DEFAULT SYSUTCDATETIME(),
    FechaActualizacion DATETIME2       NULL,
    CONSTRAINT PK_Perfiles PRIMARY KEY (Id),
    CONSTRAINT UQ_Perfiles_UsuarioId UNIQUE (UsuarioId),
    CONSTRAINT UQ_Perfiles_Correo UNIQUE (Correo)
);
GO

-- Historial de cambios del perfil 
CREATE TABLE dbo.HistorialCambiosPerfil
(
    Id            INT IDENTITY(1,1) NOT NULL,
    UsuarioId     NVARCHAR(450)     NOT NULL,
    Campo         NVARCHAR(50)      NOT NULL,
    ValorAnterior NVARCHAR(250)     NULL,
    ValorNuevo    NVARCHAR(250)     NULL,
    FechaCambio   DATETIME2         NOT NULL CONSTRAINT DF_Historial_Fecha DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_HistorialCambiosPerfil PRIMARY KEY (Id)
);
GO
