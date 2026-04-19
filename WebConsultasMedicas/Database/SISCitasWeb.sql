/*==============================================================
  BASE DE DATOS: SISCitasWeb
  DESCRIPCION : Sistema de gestión de citas médicas
  MOTOR       : SQL Server
==============================================================*/

-- Crear base de datos
IF DB_ID('SISCitasWeb') IS NULL
BEGIN
    CREATE DATABASE SISCitasWeb;
END
GO

USE SISCitasWeb;
GO

/*==============================================================
  ELIMINAR TABLAS SI EXISTEN (DESTRUCTIVO)
==============================================================*/
IF OBJECT_ID('dbo.HistorialCita', 'U') IS NOT NULL DROP TABLE dbo.HistorialCita;
IF OBJECT_ID('dbo.CitaMedica', 'U') IS NOT NULL DROP TABLE dbo.CitaMedica;
IF OBJECT_ID('dbo.HorarioMedico', 'U') IS NOT NULL DROP TABLE dbo.HorarioMedico;
IF OBJECT_ID('dbo.Paciente', 'U') IS NOT NULL DROP TABLE dbo.Paciente;
IF OBJECT_ID('dbo.Usuario', 'U') IS NOT NULL DROP TABLE dbo.Usuario;
IF OBJECT_ID('dbo.Medico', 'U') IS NOT NULL DROP TABLE dbo.Medico;
IF OBJECT_ID('dbo.Especialidad', 'U') IS NOT NULL DROP TABLE dbo.Especialidad;
IF OBJECT_ID('dbo.Turno', 'U') IS NOT NULL DROP TABLE dbo.Turno;
IF OBJECT_ID('dbo.EstadoCita', 'U') IS NOT NULL DROP TABLE dbo.EstadoCita;
IF OBJECT_ID('dbo.Rol', 'U') IS NOT NULL DROP TABLE dbo.Rol;
GO

/*==============================================================
  TABLA: Rol
==============================================================*/
CREATE TABLE dbo.Rol
(
    IdRol INT IDENTITY(1,1) NOT NULL,
    Nombre VARCHAR(50) NOT NULL,
    Estado BIT NOT NULL CONSTRAINT DF_Rol_Estado DEFAULT(1),
    CONSTRAINT PK_Rol PRIMARY KEY (IdRol),
    CONSTRAINT UQ_Rol_Nombre UNIQUE (Nombre)
);
GO

/*==============================================================
  TABLA: Usuario
==============================================================*/
CREATE TABLE dbo.Usuario
(
    IdUsuario INT IDENTITY(1,1) NOT NULL,
    Correo VARCHAR(100) NOT NULL,
    ClaveHash VARCHAR(255) NOT NULL,
    IdRol INT NOT NULL,
    Estado BIT NOT NULL CONSTRAINT DF_Usuario_Estado DEFAULT(1),
    FechaRegistro DATETIME NOT NULL CONSTRAINT DF_Usuario_FechaRegistro DEFAULT(GETDATE()),
    CONSTRAINT PK_Usuario PRIMARY KEY (IdUsuario),
    CONSTRAINT UQ_Usuario_Correo UNIQUE (Correo),
    CONSTRAINT FK_Usuario_Rol FOREIGN KEY (IdRol) REFERENCES dbo.Rol(IdRol)
);
GO

/*==============================================================
  TABLA: Paciente
==============================================================*/
CREATE TABLE dbo.Paciente
(
    IdPaciente INT IDENTITY(1,1) NOT NULL,
    IdUsuario INT NOT NULL,
    DNI CHAR(8) NOT NULL,
    Nombres VARCHAR(100) NOT NULL,
    ApellidoPaterno VARCHAR(100) NOT NULL,
    ApellidoMaterno VARCHAR(100) NOT NULL,
    FechaNacimiento DATE NOT NULL,
    Sexo CHAR(1) NOT NULL,
    Telefono VARCHAR(15) NULL,
    Direccion VARCHAR(200) NULL,
    NumeroSIS VARCHAR(20) NOT NULL,
    Estado BIT NOT NULL CONSTRAINT DF_Paciente_Estado DEFAULT(1),
    CONSTRAINT PK_Paciente PRIMARY KEY (IdPaciente),
    CONSTRAINT UQ_Paciente_IdUsuario UNIQUE (IdUsuario),
    CONSTRAINT UQ_Paciente_DNI UNIQUE (DNI),
    CONSTRAINT UQ_Paciente_NumeroSIS UNIQUE (NumeroSIS),
    CONSTRAINT FK_Paciente_Usuario FOREIGN KEY (IdUsuario) REFERENCES dbo.Usuario(IdUsuario),
    CONSTRAINT CK_Paciente_Sexo CHECK (Sexo IN ('M','F'))
);
GO

/*==============================================================
  TABLA: Especialidad
==============================================================*/
CREATE TABLE dbo.Especialidad
(
    IdEspecialidad INT IDENTITY(1,1) NOT NULL,
    Nombre VARCHAR(100) NOT NULL,
    Descripcion VARCHAR(200) NULL,
    Estado BIT NOT NULL CONSTRAINT DF_Especialidad_Estado DEFAULT(1),
    CONSTRAINT PK_Especialidad PRIMARY KEY (IdEspecialidad),
    CONSTRAINT UQ_Especialidad_Nombre UNIQUE (Nombre)
);
GO

/*==============================================================
  TABLA: Medico
==============================================================*/
CREATE TABLE dbo.Medico
(
    IdMedico INT IDENTITY(1,1) NOT NULL,
    IdEspecialidad INT NOT NULL,
    CMP VARCHAR(20) NOT NULL,
    Nombres VARCHAR(100) NOT NULL,
    ApellidoPaterno VARCHAR(100) NOT NULL,
    ApellidoMaterno VARCHAR(100) NOT NULL,
    Telefono VARCHAR(15) NULL,
    Correo VARCHAR(100) NULL,
    Estado BIT NOT NULL CONSTRAINT DF_Medico_Estado DEFAULT(1),
    CONSTRAINT PK_Medico PRIMARY KEY (IdMedico),
    CONSTRAINT UQ_Medico_CMP UNIQUE (CMP),
    CONSTRAINT FK_Medico_Especialidad FOREIGN KEY (IdEspecialidad) REFERENCES dbo.Especialidad(IdEspecialidad)
);
GO

/*==============================================================
  TABLA: Turno
==============================================================*/
CREATE TABLE dbo.Turno
(
    IdTurno INT IDENTITY(1,1) NOT NULL,
    Nombre VARCHAR(50) NOT NULL,
    HoraInicio TIME NOT NULL,
    HoraFin TIME NOT NULL,
    Estado BIT NOT NULL CONSTRAINT DF_Turno_Estado DEFAULT(1),
    CONSTRAINT PK_Turno PRIMARY KEY (IdTurno)
);
GO

/*==============================================================
  TABLA: HorarioMedico
==============================================================*/
CREATE TABLE dbo.HorarioMedico
(
    IdHorarioMedico INT IDENTITY(1,1) NOT NULL,
    IdMedico INT NOT NULL,
    IdEspecialidad INT NOT NULL,
    IdTurno INT NOT NULL,
    DiaSemana TINYINT NOT NULL,
    HoraInicio TIME NOT NULL,
    HoraFin TIME NOT NULL,
    Cupos INT NOT NULL CONSTRAINT DF_HorarioMedico_Cupos DEFAULT(10),
    Estado BIT NOT NULL CONSTRAINT DF_HorarioMedico_Estado DEFAULT(1),
    CONSTRAINT PK_HorarioMedico PRIMARY KEY (IdHorarioMedico),
    CONSTRAINT FK_HorarioMedico_Medico FOREIGN KEY (IdMedico) REFERENCES dbo.Medico(IdMedico),
    CONSTRAINT FK_HorarioMedico_Especialidad FOREIGN KEY (IdEspecialidad) REFERENCES dbo.Especialidad(IdEspecialidad),
    CONSTRAINT FK_HorarioMedico_Turno FOREIGN KEY (IdTurno) REFERENCES dbo.Turno(IdTurno),
    CONSTRAINT CK_HorarioMedico_DiaSemana CHECK (DiaSemana BETWEEN 1 AND 7),
    CONSTRAINT CK_HorarioMedico_Cupos CHECK (Cupos > 0)
);
GO

/*==============================================================
  TABLA: EstadoCita
==============================================================*/
CREATE TABLE dbo.EstadoCita
(
    IdEstadoCita INT IDENTITY(1,1) NOT NULL,
    Nombre VARCHAR(50) NOT NULL,
    CONSTRAINT PK_EstadoCita PRIMARY KEY (IdEstadoCita),
    CONSTRAINT UQ_EstadoCita_Nombre UNIQUE (Nombre)
);
GO

/*==============================================================
  TABLA: CitaMedica
==============================================================*/
CREATE TABLE dbo.CitaMedica
(
    IdCita INT IDENTITY(1,1) NOT NULL,
    IdPaciente INT NOT NULL,
    IdMedico INT NOT NULL,
    IdEspecialidad INT NOT NULL,
    IdHorarioMedico INT NOT NULL,
    IdEstadoCita INT NOT NULL,
    FechaCita DATE NOT NULL,
    HoraCita TIME NOT NULL,
    MotivoConsulta VARCHAR(250) NULL,
    Observacion VARCHAR(250) NULL,
    FechaRegistro DATETIME NOT NULL CONSTRAINT DF_CitaMedica_FechaRegistro DEFAULT(GETDATE()),
    CONSTRAINT PK_CitaMedica PRIMARY KEY (IdCita),
    CONSTRAINT FK_CitaMedica_Paciente FOREIGN KEY (IdPaciente) REFERENCES dbo.Paciente(IdPaciente),
    CONSTRAINT FK_CitaMedica_Medico FOREIGN KEY (IdMedico) REFERENCES dbo.Medico(IdMedico),
    CONSTRAINT FK_CitaMedica_Especialidad FOREIGN KEY (IdEspecialidad) REFERENCES dbo.Especialidad(IdEspecialidad),
    CONSTRAINT FK_CitaMedica_HorarioMedico FOREIGN KEY (IdHorarioMedico) REFERENCES dbo.HorarioMedico(IdHorarioMedico),
    CONSTRAINT FK_CitaMedica_EstadoCita FOREIGN KEY (IdEstadoCita) REFERENCES dbo.EstadoCita(IdEstadoCita)
);
GO

/*==============================================================
  TABLA: HistorialCita
==============================================================*/
CREATE TABLE dbo.HistorialCita
(
    IdHistorial INT IDENTITY(1,1) NOT NULL,
    IdCita INT NOT NULL,
    IdEstadoCita INT NOT NULL,
    FechaCambio DATETIME NOT NULL CONSTRAINT DF_HistorialCita_FechaCambio DEFAULT(GETDATE()),
    Observacion VARCHAR(250) NULL,
    UsuarioAccion VARCHAR(100) NULL,
    CONSTRAINT PK_HistorialCita PRIMARY KEY (IdHistorial),
    CONSTRAINT FK_HistorialCita_CitaMedica FOREIGN KEY (IdCita) REFERENCES dbo.CitaMedica(IdCita),
    CONSTRAINT FK_HistorialCita_EstadoCita FOREIGN KEY (IdEstadoCita) REFERENCES dbo.EstadoCita(IdEstadoCita)
);
GO

/*==============================================================
  INDICES
==============================================================*/
CREATE INDEX IX_CitaMedica_FechaCita ON dbo.CitaMedica(FechaCita);
CREATE INDEX IX_CitaMedica_IdPaciente ON dbo.CitaMedica(IdPaciente);
CREATE INDEX IX_CitaMedica_IdMedico ON dbo.CitaMedica(IdMedico);
CREATE INDEX IX_Medico_IdEspecialidad ON dbo.Medico(IdEspecialidad);
CREATE INDEX IX_HorarioMedico_IdMedico ON dbo.HorarioMedico(IdMedico);
CREATE INDEX IX_HorarioMedico_IdEspecialidad ON dbo.HorarioMedico(IdEspecialidad);
GO

/*==============================================================
  EVITAR DUPLICADOS DE CITAS ACTIVAS
  Estados activos:
  1 = Programada
  2 = Atendida
  4 = Reprogramada
==============================================================*/
CREATE UNIQUE INDEX IX_CitaPacienteFechaHora
ON dbo.CitaMedica(IdPaciente, FechaCita, HoraCita)
WHERE IdEstadoCita IN (1,2,4);
GO

CREATE UNIQUE INDEX IX_CitaMedicoFechaHora
ON dbo.CitaMedica(IdMedico, FechaCita, HoraCita)
WHERE IdEstadoCita IN (1,2,4);
GO

/*==============================================================
  DATOS INICIALES
==============================================================*/
INSERT INTO dbo.Rol (Nombre) VALUES ('Administrador');
INSERT INTO dbo.Rol (Nombre) VALUES ('Paciente');

INSERT INTO dbo.EstadoCita (Nombre) VALUES ('Programada');
INSERT INTO dbo.EstadoCita (Nombre) VALUES ('Atendida');
INSERT INTO dbo.EstadoCita (Nombre) VALUES ('Cancelada');
INSERT INTO dbo.EstadoCita (Nombre) VALUES ('Reprogramada');
INSERT INTO dbo.EstadoCita (Nombre) VALUES ('No Asistio');

INSERT INTO dbo.Especialidad (Nombre, Descripcion) VALUES ('Medicina General', 'Atencion general');
INSERT INTO dbo.Especialidad (Nombre, Descripcion) VALUES ('Pediatria', 'Atencion de niños');
INSERT INTO dbo.Especialidad (Nombre, Descripcion) VALUES ('Obstetricia', 'Control prenatal y salud materna');
INSERT INTO dbo.Especialidad (Nombre, Descripcion) VALUES ('Odontologia', 'Atencion dental');

INSERT INTO dbo.Turno (Nombre, HoraInicio, HoraFin) VALUES ('Mañana', '08:00:00', '12:00:00');
INSERT INTO dbo.Turno (Nombre, HoraInicio, HoraFin) VALUES ('Tarde',  '14:00:00', '18:00:00');
GO
