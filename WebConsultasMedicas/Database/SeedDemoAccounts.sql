USE SISCitasWeb;
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

/* ============================================================
   Seed DEMO (idempotente)
   - Usuario Admin ya existe: admin@siscitasweb.local / admin123
   - Crea: paciente1@siscitasweb.local / paciente123
   - Crea: doctor1@siscitasweb.local / doctor123 (rol Medico) + registro en Medico
============================================================ */

-- Rol Medico
IF NOT EXISTS (SELECT 1 FROM dbo.Rol WHERE Nombre = 'Medico')
BEGIN
    SET IDENTITY_INSERT dbo.Rol ON;
    INSERT INTO dbo.Rol (IdRol, Nombre, Estado) VALUES (3, 'Medico', 1);
    SET IDENTITY_INSERT dbo.Rol OFF;
END
GO

-- Paciente demo
IF NOT EXISTS (SELECT 1 FROM dbo.Usuario WHERE Correo = 'paciente1@siscitasweb.local')
BEGIN
    DECLARE @PacienteIdUsuario INT;
    INSERT INTO dbo.Usuario (Correo, ClaveHash, IdRol, Estado)
    VALUES ('paciente1@siscitasweb.local',
            '299fbb455c42239c86d2ee3b15403ed1b468259ecaedf0c3527451e1f0d63d59', -- sha256("paciente123")
            2, 1);
    SET @PacienteIdUsuario = SCOPE_IDENTITY();

    INSERT INTO dbo.Paciente (IdUsuario, DNI, Nombres, ApellidoPaterno, ApellidoMaterno, FechaNacimiento, Sexo, Telefono, Direccion, NumeroSIS, Estado)
    VALUES (@PacienteIdUsuario, '12345678', 'Juan', 'Perez', 'Gomez', '1998-05-10', 'M', '999999999', 'Lima', 'SIS0000001', 1);
END
GO

-- Doctor demo + Medico
IF NOT EXISTS (SELECT 1 FROM dbo.Usuario WHERE Correo = 'doctor1@siscitasweb.local')
BEGIN
    DECLARE @DoctorIdUsuario INT;
    INSERT INTO dbo.Usuario (Correo, ClaveHash, IdRol, Estado)
    VALUES ('doctor1@siscitasweb.local',
            'f348d5628621f3d8f59c8cabda0f8eb0aa7e0514a90be7571020b1336f26c113', -- sha256("doctor123")
            3, 1);
    SET @DoctorIdUsuario = SCOPE_IDENTITY();

    IF NOT EXISTS (SELECT 1 FROM dbo.Medico WHERE IdUsuario = @DoctorIdUsuario)
    BEGIN
        -- Usa Especialidad 1 (Medicina General) si existe; si no, toma la primera.
        DECLARE @IdEspecialidad INT = (SELECT TOP 1 IdEspecialidad FROM dbo.Especialidad ORDER BY IdEspecialidad);

        INSERT INTO dbo.Medico (IdUsuario, IdEspecialidad, CMP, Nombres, ApellidoPaterno, ApellidoMaterno, Telefono, Correo, Estado)
        VALUES (@DoctorIdUsuario, @IdEspecialidad, 'CMP0001', 'Carlos', 'Ramirez', 'Torres', '988888888', 'doctor1@siscitasweb.local', 1);
    END
END
GO
