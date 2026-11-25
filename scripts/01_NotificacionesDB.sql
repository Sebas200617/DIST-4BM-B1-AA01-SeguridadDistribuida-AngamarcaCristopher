-- Crear la base de datos
    CREATE DATABASE NotificacionesDB;
GO

USE NotificacionesDB;
GO

-- Tabla de envíos de correo (no guarda tokens ni contraseñas)

CREATE TABLE dbo.EnviosCorreo
(
    Id            INT IDENTITY(1,1) NOT NULL,
    UsuarioId     NVARCHAR(450)     NULL,
    Destinatario  NVARCHAR(256)     NOT NULL,
    Asunto        NVARCHAR(200)     NOT NULL,
    Tipo          NVARCHAR(50)      NOT NULL,   -- Bienvenida, Recuperacion, AvisoPerfil
    Estado        NVARCHAR(20)      NOT NULL,   -- Enviado o Fallido
    MensajeError  NVARCHAR(500)     NULL,
    FechaEnvio    DATETIME2         NOT NULL CONSTRAINT DF_EnviosCorreo_Fecha DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_EnviosCorreo PRIMARY KEY (Id)
);
GO
