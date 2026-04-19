IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260418190346_InitialCreate'
)
BEGIN
    CREATE TABLE [Especialidad] (
        [IdEspecialidad] int NOT NULL IDENTITY,
        [Nombre] varchar(100) NOT NULL,
        [Descripcion] varchar(200) NULL,
        [Estado] bit NOT NULL DEFAULT CAST(1 AS bit),
        CONSTRAINT [PK_Especialidad] PRIMARY KEY ([IdEspecialidad])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260418190346_InitialCreate'
)
BEGIN
    CREATE TABLE [EstadoCita] (
        [IdEstadoCita] int NOT NULL IDENTITY,
        [Nombre] varchar(50) NOT NULL,
        CONSTRAINT [PK_EstadoCita] PRIMARY KEY ([IdEstadoCita])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260418190346_InitialCreate'
)
BEGIN
    CREATE TABLE [Medico] (
        [IdMedico] int NOT NULL IDENTITY,
        [CMP] varchar(20) NOT NULL,
        [Nombres] varchar(100) NOT NULL,
        [ApellidoPaterno] varchar(100) NOT NULL,
        [ApellidoMaterno] varchar(100) NOT NULL,
        [Telefono] varchar(15) NULL,
        [Correo] varchar(100) NULL,
        [Estado] bit NOT NULL DEFAULT CAST(1 AS bit),
        CONSTRAINT [PK_Medico] PRIMARY KEY ([IdMedico])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260418190346_InitialCreate'
)
BEGIN
    CREATE TABLE [Rol] (
        [IdRol] int NOT NULL IDENTITY,
        [Nombre] varchar(50) NOT NULL,
        [Estado] bit NOT NULL DEFAULT CAST(1 AS bit),
        CONSTRAINT [PK_Rol] PRIMARY KEY ([IdRol])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260418190346_InitialCreate'
)
BEGIN
    CREATE TABLE [Turno] (
        [IdTurno] int NOT NULL IDENTITY,
        [Nombre] varchar(50) NOT NULL,
        [HoraInicio] time NOT NULL,
        [HoraFin] time NOT NULL,
        [Estado] bit NOT NULL DEFAULT CAST(1 AS bit),
        CONSTRAINT [PK_Turno] PRIMARY KEY ([IdTurno])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260418190346_InitialCreate'
)
BEGIN
    CREATE TABLE [MedicoEspecialidad] (
        [IdMedicoEspecialidad] int NOT NULL IDENTITY,
        [IdMedico] int NOT NULL,
        [IdEspecialidad] int NOT NULL,
        [Estado] bit NOT NULL DEFAULT CAST(1 AS bit),
        CONSTRAINT [PK_MedicoEspecialidad] PRIMARY KEY ([IdMedicoEspecialidad]),
        CONSTRAINT [FK_MedicoEspecialidad_Especialidad] FOREIGN KEY ([IdEspecialidad]) REFERENCES [Especialidad] ([IdEspecialidad]),
        CONSTRAINT [FK_MedicoEspecialidad_Medico] FOREIGN KEY ([IdMedico]) REFERENCES [Medico] ([IdMedico])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260418190346_InitialCreate'
)
BEGIN
    CREATE TABLE [Usuario] (
        [IdUsuario] int NOT NULL IDENTITY,
        [Correo] varchar(100) NOT NULL,
        [ClaveHash] varchar(255) NOT NULL,
        [IdRol] int NOT NULL,
        [Estado] bit NOT NULL DEFAULT CAST(1 AS bit),
        [FechaRegistro] datetime NOT NULL DEFAULT (GETDATE()),
        CONSTRAINT [PK_Usuario] PRIMARY KEY ([IdUsuario]),
        CONSTRAINT [FK_Usuario_Rol] FOREIGN KEY ([IdRol]) REFERENCES [Rol] ([IdRol])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260418190346_InitialCreate'
)
BEGIN
    CREATE TABLE [HorarioMedico] (
        [IdHorarioMedico] int NOT NULL IDENTITY,
        [IdMedico] int NOT NULL,
        [IdEspecialidad] int NOT NULL,
        [IdTurno] int NOT NULL,
        [DiaSemana] tinyint NOT NULL,
        [HoraInicio] time NOT NULL,
        [HoraFin] time NOT NULL,
        [Cupos] int NOT NULL DEFAULT 10,
        [Estado] bit NOT NULL DEFAULT CAST(1 AS bit),
        CONSTRAINT [PK_HorarioMedico] PRIMARY KEY ([IdHorarioMedico]),
        CONSTRAINT [CK_HorarioMedico_Cupos] CHECK ([Cupos] > 0),
        CONSTRAINT [CK_HorarioMedico_DiaSemana] CHECK ([DiaSemana] BETWEEN 1 AND 7),
        CONSTRAINT [FK_HorarioMedico_Especialidad] FOREIGN KEY ([IdEspecialidad]) REFERENCES [Especialidad] ([IdEspecialidad]),
        CONSTRAINT [FK_HorarioMedico_Medico] FOREIGN KEY ([IdMedico]) REFERENCES [Medico] ([IdMedico]),
        CONSTRAINT [FK_HorarioMedico_Turno] FOREIGN KEY ([IdTurno]) REFERENCES [Turno] ([IdTurno])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260418190346_InitialCreate'
)
BEGIN
    CREATE TABLE [Paciente] (
        [IdPaciente] int NOT NULL IDENTITY,
        [IdUsuario] int NOT NULL,
        [DNI] char(8) NOT NULL,
        [Nombres] varchar(100) NOT NULL,
        [ApellidoPaterno] varchar(100) NOT NULL,
        [ApellidoMaterno] varchar(100) NOT NULL,
        [FechaNacimiento] date NOT NULL,
        [Sexo] char(1) NOT NULL,
        [Telefono] varchar(15) NULL,
        [Direccion] varchar(200) NULL,
        [NumeroSIS] varchar(20) NOT NULL,
        [Estado] bit NOT NULL DEFAULT CAST(1 AS bit),
        CONSTRAINT [PK_Paciente] PRIMARY KEY ([IdPaciente]),
        CONSTRAINT [CK_Paciente_Sexo] CHECK ([Sexo] IN ('M','F')),
        CONSTRAINT [FK_Paciente_Usuario] FOREIGN KEY ([IdUsuario]) REFERENCES [Usuario] ([IdUsuario])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260418190346_InitialCreate'
)
BEGIN
    CREATE TABLE [CitaMedica] (
        [IdCita] int NOT NULL IDENTITY,
        [IdPaciente] int NOT NULL,
        [IdMedico] int NOT NULL,
        [IdEspecialidad] int NOT NULL,
        [IdHorarioMedico] int NOT NULL,
        [IdEstadoCita] int NOT NULL,
        [FechaCita] date NOT NULL,
        [HoraCita] time NOT NULL,
        [MotivoConsulta] varchar(250) NULL,
        [Observacion] varchar(250) NULL,
        [FechaRegistro] datetime NOT NULL DEFAULT (GETDATE()),
        CONSTRAINT [PK_CitaMedica] PRIMARY KEY ([IdCita]),
        CONSTRAINT [FK_CitaMedica_Especialidad] FOREIGN KEY ([IdEspecialidad]) REFERENCES [Especialidad] ([IdEspecialidad]),
        CONSTRAINT [FK_CitaMedica_EstadoCita] FOREIGN KEY ([IdEstadoCita]) REFERENCES [EstadoCita] ([IdEstadoCita]),
        CONSTRAINT [FK_CitaMedica_HorarioMedico] FOREIGN KEY ([IdHorarioMedico]) REFERENCES [HorarioMedico] ([IdHorarioMedico]),
        CONSTRAINT [FK_CitaMedica_Medico] FOREIGN KEY ([IdMedico]) REFERENCES [Medico] ([IdMedico]),
        CONSTRAINT [FK_CitaMedica_Paciente] FOREIGN KEY ([IdPaciente]) REFERENCES [Paciente] ([IdPaciente])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260418190346_InitialCreate'
)
BEGIN
    CREATE TABLE [HistorialCita] (
        [IdHistorial] int NOT NULL IDENTITY,
        [IdCita] int NOT NULL,
        [IdEstadoCita] int NOT NULL,
        [FechaCambio] datetime NOT NULL DEFAULT (GETDATE()),
        [Observacion] varchar(250) NULL,
        [UsuarioAccion] varchar(100) NULL,
        CONSTRAINT [PK_HistorialCita] PRIMARY KEY ([IdHistorial]),
        CONSTRAINT [FK_HistorialCita_CitaMedica] FOREIGN KEY ([IdCita]) REFERENCES [CitaMedica] ([IdCita]),
        CONSTRAINT [FK_HistorialCita_EstadoCita] FOREIGN KEY ([IdEstadoCita]) REFERENCES [EstadoCita] ([IdEstadoCita])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260418190346_InitialCreate'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'IdEspecialidad', N'Descripcion', N'Estado', N'Nombre') AND [object_id] = OBJECT_ID(N'[Especialidad]'))
        SET IDENTITY_INSERT [Especialidad] ON;
    EXEC(N'INSERT INTO [Especialidad] ([IdEspecialidad], [Descripcion], [Estado], [Nombre])
    VALUES (1, ''Atencion general'', CAST(1 AS bit), ''Medicina General''),
    (2, ''Atencion de niños'', CAST(1 AS bit), ''Pediatria''),
    (3, ''Control prenatal y salud materna'', CAST(1 AS bit), ''Obstetricia''),
    (4, ''Atencion dental'', CAST(1 AS bit), ''Odontologia'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'IdEspecialidad', N'Descripcion', N'Estado', N'Nombre') AND [object_id] = OBJECT_ID(N'[Especialidad]'))
        SET IDENTITY_INSERT [Especialidad] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260418190346_InitialCreate'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'IdEstadoCita', N'Nombre') AND [object_id] = OBJECT_ID(N'[EstadoCita]'))
        SET IDENTITY_INSERT [EstadoCita] ON;
    EXEC(N'INSERT INTO [EstadoCita] ([IdEstadoCita], [Nombre])
    VALUES (1, ''Programada''),
    (2, ''Atendida''),
    (3, ''Cancelada''),
    (4, ''Reprogramada''),
    (5, ''No Asistio'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'IdEstadoCita', N'Nombre') AND [object_id] = OBJECT_ID(N'[EstadoCita]'))
        SET IDENTITY_INSERT [EstadoCita] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260418190346_InitialCreate'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'IdRol', N'Estado', N'Nombre') AND [object_id] = OBJECT_ID(N'[Rol]'))
        SET IDENTITY_INSERT [Rol] ON;
    EXEC(N'INSERT INTO [Rol] ([IdRol], [Estado], [Nombre])
    VALUES (1, CAST(1 AS bit), ''Administrador''),
    (2, CAST(1 AS bit), ''Paciente'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'IdRol', N'Estado', N'Nombre') AND [object_id] = OBJECT_ID(N'[Rol]'))
        SET IDENTITY_INSERT [Rol] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260418190346_InitialCreate'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'IdTurno', N'Estado', N'HoraFin', N'HoraInicio', N'Nombre') AND [object_id] = OBJECT_ID(N'[Turno]'))
        SET IDENTITY_INSERT [Turno] ON;
    EXEC(N'INSERT INTO [Turno] ([IdTurno], [Estado], [HoraFin], [HoraInicio], [Nombre])
    VALUES (1, CAST(1 AS bit), ''12:00:00'', ''08:00:00'', ''Mañana''),
    (2, CAST(1 AS bit), ''18:00:00'', ''14:00:00'', ''Tarde'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'IdTurno', N'Estado', N'HoraFin', N'HoraInicio', N'Nombre') AND [object_id] = OBJECT_ID(N'[Turno]'))
        SET IDENTITY_INSERT [Turno] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260418190346_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_CitaMedica_FechaCita] ON [CitaMedica] ([FechaCita]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260418190346_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_CitaMedica_IdEspecialidad] ON [CitaMedica] ([IdEspecialidad]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260418190346_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_CitaMedica_IdEstadoCita] ON [CitaMedica] ([IdEstadoCita]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260418190346_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_CitaMedica_IdHorarioMedico] ON [CitaMedica] ([IdHorarioMedico]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260418190346_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_CitaMedica_IdMedico] ON [CitaMedica] ([IdMedico]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260418190346_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_CitaMedica_IdPaciente] ON [CitaMedica] ([IdPaciente]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260418190346_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_CitaMedicoFechaHora] ON [CitaMedica] ([IdMedico], [FechaCita], [HoraCita]) WHERE [IdEstadoCita] IN (1,2,4)');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260418190346_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_CitaPacienteFechaHora] ON [CitaMedica] ([IdPaciente], [FechaCita], [HoraCita]) WHERE [IdEstadoCita] IN (1,2,4)');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260418190346_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [UQ_Especialidad_Nombre] ON [Especialidad] ([Nombre]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260418190346_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [UQ_EstadoCita_Nombre] ON [EstadoCita] ([Nombre]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260418190346_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_HistorialCita_IdCita] ON [HistorialCita] ([IdCita]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260418190346_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_HistorialCita_IdEstadoCita] ON [HistorialCita] ([IdEstadoCita]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260418190346_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_HorarioMedico_IdEspecialidad] ON [HorarioMedico] ([IdEspecialidad]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260418190346_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_HorarioMedico_IdMedico] ON [HorarioMedico] ([IdMedico]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260418190346_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_HorarioMedico_IdTurno] ON [HorarioMedico] ([IdTurno]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260418190346_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [UQ_Medico_CMP] ON [Medico] ([CMP]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260418190346_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_MedicoEspecialidad_IdEspecialidad] ON [MedicoEspecialidad] ([IdEspecialidad]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260418190346_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [UQ_MedicoEspecialidad] ON [MedicoEspecialidad] ([IdMedico], [IdEspecialidad]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260418190346_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [UQ_Paciente_DNI] ON [Paciente] ([DNI]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260418190346_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [UQ_Paciente_IdUsuario] ON [Paciente] ([IdUsuario]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260418190346_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [UQ_Paciente_NumeroSIS] ON [Paciente] ([NumeroSIS]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260418190346_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [UQ_Rol_Nombre] ON [Rol] ([Nombre]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260418190346_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Usuario_IdRol] ON [Usuario] ([IdRol]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260418190346_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [UQ_Usuario_Correo] ON [Usuario] ([Correo]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260418190346_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260418190346_InitialCreate', N'8.0.11');
END;
GO

COMMIT;
GO

