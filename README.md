# citas-postas-medicas-app

Aplicación web de gestión de citas para postas médicas y consultorios, desarrollada con ASP.NET Core MVC y Entity Framework Core sobre SQL Server.

## Estado actual del proyecto

El sistema ya cuenta con un panel administrativo funcional y módulos CRUD para la gestión de catálogos, agenda médica, pacientes y citas. La aplicación inicia en la pantalla de login y redirige al panel administrativo.

Nota actual:

- El formulario de login ya existe a nivel visual, pero la validación real de credenciales y la gestión de sesión todavía no están implementadas. Hoy el `POST /Auth/Login` redirige directamente al dashboard.

## Funcionalidades implementadas

### 1. Acceso y panel principal

- Pantalla de inicio de sesión con interfaz personalizada.
- Panel administrativo principal.
- Layout administrativo separado del layout público.

### 2. Gestión de seguridad y usuarios

- CRUD de roles.
- CRUD de usuarios.
- Relación de usuario con rol.
- Hash de contraseña con SHA-256 al crear o actualizar usuarios.
- Usuario administrador inicial sembrado en base de datos.

Credenciales iniciales sembradas:

- Correo: `admin@siscitasweb.local`
- Contraseña base usada para el hash inicial: `admin123`

### 3. Gestión de pacientes

- CRUD de pacientes.
- Asociación uno a uno entre paciente y usuario.
- Registro de datos personales:
  - DNI
  - nombres y apellidos
  - fecha de nacimiento
  - sexo
  - teléfono
  - dirección
  - número SIS
- Validación de unicidad para DNI, número SIS y usuario asignado.

### 4. Gestión de especialidades y médicos

- CRUD de especialidades médicas.
- CRUD de médicos.
- Asociación de cada médico a una especialidad.
- Validación de unicidad para CMP.

Especialidades semilla actuales:

- Medicina General
- Pediatría
- Obstetricia
- Odontología

### 5. Gestión de turnos y horarios médicos

- CRUD de turnos.
- CRUD de horarios médicos.
- Asociación de horarios con médico, especialidad y turno.
- Configuración de:
  - día de semana
  - hora de inicio
  - hora de fin
  - cupos
  - estado
- Asignación automática de especialidad en el horario según el médico seleccionado.

Turnos semilla actuales:

- Mañana
- Tarde

### 6. Gestión de citas médicas

- CRUD de citas médicas.
- Asociación de la cita con:
  - paciente
  - médico
  - especialidad
  - horario médico
  - estado de cita
- Registro de fecha, hora, motivo de consulta y observación.
- Asignación automática de médico y especialidad a partir del horario seleccionado.
- Listado de citas con joins a las tablas relacionadas.
- Control de conflictos mediante restricciones únicas en base de datos para evitar:
  - duplicidad de citas del mismo paciente en la misma fecha y hora
  - duplicidad de citas del mismo médico en la misma fecha y hora

### 7. Gestión de estados e historial de citas

- CRUD de estados de cita.
- CRUD de historial de citas.
- Registro de cambios de estado con fecha, observación y usuario de acción.
- Relación del historial con la cita y el estado asociado.

Estados de cita sembrados:

- Programada
- Atendida
- Cancelada
- Reprogramada
- No Asistio

## Modelo de datos actual

Entidades principales incluidas en el proyecto:

- `Rol`
- `Usuario`
- `Paciente`
- `Especialidad`
- `Medico`
- `Turno`
- `HorarioMedico`
- `EstadoCita`
- `CitaMedica`
- `HistorialCita`

Relaciones importantes:

- Un `Usuario` pertenece a un `Rol`.
- Un `Paciente` está asociado a un único `Usuario`.
- Un `Medico` pertenece a una `Especialidad`.
- Un `HorarioMedico` pertenece a un `Medico`, una `Especialidad` y un `Turno`.
- Una `CitaMedica` pertenece a un `Paciente`, `Medico`, `Especialidad`, `HorarioMedico` y `EstadoCita`.
- Un `HistorialCita` pertenece a una `CitaMedica` y a un `EstadoCita`.

## Tecnologías usadas

- .NET 8
- ASP.NET Core MVC
- Entity Framework Core 8
- SQL Server
- Razor Views
- Bootstrap

## Estructura del proyecto

```text
WebConsultasMedicas.sln
README.md
WebConsultasMedicas/
  Controllers/
  Data/
  Database/
  Migrations/
  Models/
  Security/
  Views/
  wwwroot/
  Program.cs
  appsettings.json
```

## Base de datos

La aplicación está configurada actualmente para trabajar con SQL Server Express usando esta cadena de conexión:

```json
"DefaultConnection": "Server=.\\SQLEXPRESS;Database=SISCitasWeb;Trusted_Connection=True;MultipleActiveResultSets=true;Encrypt=False;TrustServerCertificate=True"
```

Archivos de apoyo incluidos:

- `WebConsultasMedicas/Database/SISCitasWeb.sql`: script completo de creación de la base de datos.
- `WebConsultasMedicas/Database/EF_Migrations_Idempotent.sql`: script idempotente de migraciones.
- Carpeta `WebConsultasMedicas/Migrations/` con migraciones de Entity Framework Core.

## Cómo ejecutar el proyecto

### Requisitos

- .NET 8 SDK
- SQL Server o SQL Server Express
- Visual Studio 2022 o CLI de `dotnet`

### Pasos

1. Clonar el repositorio.
2. Abrir la solución `WebConsultasMedicas.sln`.
3. Revisar o ajustar la cadena de conexión en `WebConsultasMedicas/appsettings.json`.
4. Crear la base de datos con una de estas opciones:
   - ejecutar el script `WebConsultasMedicas/Database/SISCitasWeb.sql`
   - aplicar las migraciones de Entity Framework Core
5. Ejecutar el proyecto.

### Comandos útiles

```bash
dotnet restore
dotnet build
dotnet run --project WebConsultasMedicas/WebConsultasMedicas.csproj
```

Si deseas crear la base con migraciones:

```bash
dotnet ef database update --project WebConsultasMedicas/WebConsultasMedicas.csproj
```

## Validaciones y reglas ya implementadas

- Unicidad en nombres de roles.
- Unicidad en correo de usuario.
- Unicidad en DNI y número SIS de paciente.
- Unicidad en nombre de especialidad.
- Unicidad en CMP de médico.
- Unicidad en nombre de estado de cita.
- Restricción de sexo en paciente: `M` o `F`.
- Restricción de día de semana en horario médico: valores del 1 al 7.
- Restricción de cupos en horario médico: mayores a 0.
- Restricciones para evitar conflictos de agenda en citas activas.

## Pendientes o mejoras naturales siguientes

- Implementar autenticación real y manejo de sesión.
- Restringir acceso por rol y autorización.
- Automatizar el registro del historial cuando cambia el estado de una cita.
- Validar disponibilidad de cupos desde la lógica de aplicación.
- Agregar búsqueda, filtros y paginación.
- Incorporar pruebas automatizadas.

## Autoría

Proyecto orientado a la gestión de citas médicas para postas y consultorios, enfocado en organizar pacientes, médicos, horarios y seguimiento de atención.
