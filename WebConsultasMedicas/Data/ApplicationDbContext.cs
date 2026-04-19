using Microsoft.EntityFrameworkCore;
using WebConsultasMedicas.Models;
using WebConsultasMedicas.Security;

namespace WebConsultasMedicas.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Rol> Roles => Set<Rol>();
        public DbSet<Usuario> Usuarios => Set<Usuario>();
        public DbSet<Paciente> Pacientes => Set<Paciente>();
        public DbSet<Especialidad> Especialidades => Set<Especialidad>();
        public DbSet<Medico> Medicos => Set<Medico>();
        public DbSet<Turno> Turnos => Set<Turno>();
        public DbSet<HorarioMedico> HorariosMedicos => Set<HorarioMedico>();
        public DbSet<EstadoCita> EstadosCita => Set<EstadoCita>();
        public DbSet<CitaMedica> CitasMedicas => Set<CitaMedica>();
        public DbSet<HistorialCita> HistorialCitas => Set<HistorialCita>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Rol>(entity =>
            {
                entity.ToTable("Rol");
                entity.HasKey(e => e.IdRol).HasName("PK_Rol");
                entity.Property(e => e.Nombre).HasMaxLength(50).IsUnicode(false).IsRequired();
                entity.Property(e => e.Estado).HasDefaultValue(true);
                entity.HasIndex(e => e.Nombre).IsUnique().HasDatabaseName("UQ_Rol_Nombre");
            });

            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.ToTable("Usuario");
                entity.HasKey(e => e.IdUsuario).HasName("PK_Usuario");
                entity.Property(e => e.Correo).HasMaxLength(100).IsUnicode(false).IsRequired();
                entity.Property(e => e.ClaveHash).HasMaxLength(255).IsUnicode(false).IsRequired();
                entity.Property(e => e.Estado).HasDefaultValue(true);
                entity.Property(e => e.FechaRegistro).HasColumnType("datetime").HasDefaultValueSql("GETDATE()");
                entity.HasIndex(e => e.Correo).IsUnique().HasDatabaseName("UQ_Usuario_Correo");

                entity.HasOne(e => e.Rol)
                    .WithMany(r => r.Usuarios)
                    .HasForeignKey(e => e.IdRol)
                    .HasConstraintName("FK_Usuario_Rol")
                    .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<Paciente>(entity =>
            {
                entity.ToTable("Paciente", tb =>
                {
                    tb.HasCheckConstraint("CK_Paciente_Sexo", "[Sexo] IN ('M','F')");
                });
                entity.HasKey(e => e.IdPaciente).HasName("PK_Paciente");
                entity.Property(e => e.DNI).HasColumnType("char(8)").IsUnicode(false).IsFixedLength().IsRequired();
                entity.Property(e => e.Nombres).HasMaxLength(100).IsUnicode(false).IsRequired();
                entity.Property(e => e.ApellidoPaterno).HasMaxLength(100).IsUnicode(false).IsRequired();
                entity.Property(e => e.ApellidoMaterno).HasMaxLength(100).IsUnicode(false).IsRequired();
                entity.Property(e => e.FechaNacimiento).HasColumnType("date").IsRequired();
                entity.Property(e => e.Sexo).HasColumnType("char(1)").IsUnicode(false).IsFixedLength().IsRequired();
                entity.Property(e => e.Telefono).HasMaxLength(15).IsUnicode(false);
                entity.Property(e => e.Direccion).HasMaxLength(200).IsUnicode(false);
                entity.Property(e => e.NumeroSIS).HasMaxLength(20).IsUnicode(false).IsRequired();
                entity.Property(e => e.Estado).HasDefaultValue(true);

                entity.HasIndex(e => e.IdUsuario).IsUnique().HasDatabaseName("UQ_Paciente_IdUsuario");
                entity.HasIndex(e => e.DNI).IsUnique().HasDatabaseName("UQ_Paciente_DNI");
                entity.HasIndex(e => e.NumeroSIS).IsUnique().HasDatabaseName("UQ_Paciente_NumeroSIS");

                entity.HasOne(e => e.Usuario)
                    .WithOne(u => u.Paciente)
                    .HasForeignKey<Paciente>(e => e.IdUsuario)
                    .HasConstraintName("FK_Paciente_Usuario")
                    .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<Especialidad>(entity =>
            {
                entity.ToTable("Especialidad");
                entity.HasKey(e => e.IdEspecialidad).HasName("PK_Especialidad");
                entity.Property(e => e.Nombre).HasMaxLength(100).IsUnicode(false).IsRequired();
                entity.Property(e => e.Descripcion).HasMaxLength(200).IsUnicode(false);
                entity.Property(e => e.Estado).HasDefaultValue(true);
                entity.HasIndex(e => e.Nombre).IsUnique().HasDatabaseName("UQ_Especialidad_Nombre");
            });

            modelBuilder.Entity<Medico>(entity =>
            {
                entity.ToTable("Medico");
                entity.HasKey(e => e.IdMedico).HasName("PK_Medico");
                entity.Property(e => e.IdEspecialidad).IsRequired();
                entity.Property(e => e.CMP).HasMaxLength(20).IsUnicode(false).IsRequired();
                entity.Property(e => e.Nombres).HasMaxLength(100).IsUnicode(false).IsRequired();
                entity.Property(e => e.ApellidoPaterno).HasMaxLength(100).IsUnicode(false).IsRequired();
                entity.Property(e => e.ApellidoMaterno).HasMaxLength(100).IsUnicode(false).IsRequired();
                entity.Property(e => e.Telefono).HasMaxLength(15).IsUnicode(false);
                entity.Property(e => e.Correo).HasMaxLength(100).IsUnicode(false);
                entity.Property(e => e.Estado).HasDefaultValue(true);
                entity.HasIndex(e => e.CMP).IsUnique().HasDatabaseName("UQ_Medico_CMP");
                entity.HasIndex(e => e.IdEspecialidad).HasDatabaseName("IX_Medico_IdEspecialidad");

                entity.HasOne(e => e.Especialidad)
                    .WithMany(es => es.Medicos)
                    .HasForeignKey(e => e.IdEspecialidad)
                    .HasConstraintName("FK_Medico_Especialidad")
                    .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<Turno>(entity =>
            {
                entity.ToTable("Turno");
                entity.HasKey(e => e.IdTurno).HasName("PK_Turno");
                entity.Property(e => e.Nombre).HasMaxLength(50).IsUnicode(false).IsRequired();
                entity.Property(e => e.HoraInicio).HasColumnType("time").IsRequired();
                entity.Property(e => e.HoraFin).HasColumnType("time").IsRequired();
                entity.Property(e => e.Estado).HasDefaultValue(true);
            });

            modelBuilder.Entity<HorarioMedico>(entity =>
            {
                entity.ToTable("HorarioMedico", tb =>
                {
                    tb.HasCheckConstraint("CK_HorarioMedico_DiaSemana", "[DiaSemana] BETWEEN 1 AND 7");
                    tb.HasCheckConstraint("CK_HorarioMedico_Cupos", "[Cupos] > 0");
                });
                entity.HasKey(e => e.IdHorarioMedico).HasName("PK_HorarioMedico");
                entity.Property(e => e.DiaSemana).HasColumnType("tinyint").IsRequired();
                entity.Property(e => e.HoraInicio).HasColumnType("time").IsRequired();
                entity.Property(e => e.HoraFin).HasColumnType("time").IsRequired();
                entity.Property(e => e.Cupos).HasDefaultValue(10);
                entity.Property(e => e.Estado).HasDefaultValue(true);

                entity.HasIndex(e => e.IdMedico).HasDatabaseName("IX_HorarioMedico_IdMedico");
                entity.HasIndex(e => e.IdEspecialidad).HasDatabaseName("IX_HorarioMedico_IdEspecialidad");

                entity.HasOne(e => e.Medico)
                    .WithMany(m => m.Horarios)
                    .HasForeignKey(e => e.IdMedico)
                    .HasConstraintName("FK_HorarioMedico_Medico")
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.Especialidad)
                    .WithMany(es => es.Horarios)
                    .HasForeignKey(e => e.IdEspecialidad)
                    .HasConstraintName("FK_HorarioMedico_Especialidad")
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.Turno)
                    .WithMany(t => t.Horarios)
                    .HasForeignKey(e => e.IdTurno)
                    .HasConstraintName("FK_HorarioMedico_Turno")
                    .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<EstadoCita>(entity =>
            {
                entity.ToTable("EstadoCita");
                entity.HasKey(e => e.IdEstadoCita).HasName("PK_EstadoCita");
                entity.Property(e => e.Nombre).HasMaxLength(50).IsUnicode(false).IsRequired();
                entity.HasIndex(e => e.Nombre).IsUnique().HasDatabaseName("UQ_EstadoCita_Nombre");
            });

            modelBuilder.Entity<CitaMedica>(entity =>
            {
                entity.ToTable("CitaMedica");
                entity.HasKey(e => e.IdCita).HasName("PK_CitaMedica");
                entity.Property(e => e.FechaCita).HasColumnType("date").IsRequired();
                entity.Property(e => e.HoraCita).HasColumnType("time").IsRequired();
                entity.Property(e => e.MotivoConsulta).HasMaxLength(250).IsUnicode(false);
                entity.Property(e => e.Observacion).HasMaxLength(250).IsUnicode(false);
                entity.Property(e => e.FechaRegistro).HasColumnType("datetime").HasDefaultValueSql("GETDATE()");

                entity.HasIndex(e => e.FechaCita).HasDatabaseName("IX_CitaMedica_FechaCita");
                entity.HasIndex(e => e.IdPaciente).HasDatabaseName("IX_CitaMedica_IdPaciente");
                entity.HasIndex(e => e.IdMedico).HasDatabaseName("IX_CitaMedica_IdMedico");

                entity.HasIndex(e => new { e.IdPaciente, e.FechaCita, e.HoraCita })
                    .IsUnique()
                    .HasDatabaseName("IX_CitaPacienteFechaHora")
                    .HasFilter("[IdEstadoCita] IN (1,2,4)");

                entity.HasIndex(e => new { e.IdMedico, e.FechaCita, e.HoraCita })
                    .IsUnique()
                    .HasDatabaseName("IX_CitaMedicoFechaHora")
                    .HasFilter("[IdEstadoCita] IN (1,2,4)");

                entity.HasOne(e => e.Paciente)
                    .WithMany(p => p.Citas)
                    .HasForeignKey(e => e.IdPaciente)
                    .HasConstraintName("FK_CitaMedica_Paciente")
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.Medico)
                    .WithMany(m => m.Citas)
                    .HasForeignKey(e => e.IdMedico)
                    .HasConstraintName("FK_CitaMedica_Medico")
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.Especialidad)
                    .WithMany(es => es.Citas)
                    .HasForeignKey(e => e.IdEspecialidad)
                    .HasConstraintName("FK_CitaMedica_Especialidad")
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.HorarioMedico)
                    .WithMany(h => h.Citas)
                    .HasForeignKey(e => e.IdHorarioMedico)
                    .HasConstraintName("FK_CitaMedica_HorarioMedico")
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.EstadoCita)
                    .WithMany(ec => ec.Citas)
                    .HasForeignKey(e => e.IdEstadoCita)
                    .HasConstraintName("FK_CitaMedica_EstadoCita")
                    .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<HistorialCita>(entity =>
            {
                entity.ToTable("HistorialCita");
                entity.HasKey(e => e.IdHistorial).HasName("PK_HistorialCita");
                entity.Property(e => e.FechaCambio).HasColumnType("datetime").HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.Observacion).HasMaxLength(250).IsUnicode(false);
                entity.Property(e => e.UsuarioAccion).HasMaxLength(100).IsUnicode(false);

                entity.HasOne(e => e.CitaMedica)
                    .WithMany(c => c.Historial)
                    .HasForeignKey(e => e.IdCita)
                    .HasConstraintName("FK_HistorialCita_CitaMedica")
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.EstadoCita)
                    .WithMany(ec => ec.Historiales)
                    .HasForeignKey(e => e.IdEstadoCita)
                    .HasConstraintName("FK_HistorialCita_EstadoCita")
                    .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<Rol>().HasData(
                new Rol { IdRol = 1, Nombre = "Administrador", Estado = true },
                new Rol { IdRol = 2, Nombre = "Paciente", Estado = true }
            );

            modelBuilder.Entity<Usuario>().HasData(
                new Usuario
                {
                    IdUsuario = 1,
                    Correo = "admin@siscitasweb.local",
                    ClaveHash = PasswordHasher.Sha256Hex("admin123"),
                    IdRol = 1,
                    Estado = true,
                    FechaRegistro = new DateTime(2026, 4, 18, 0, 0, 0)
                }
            );

            modelBuilder.Entity<EstadoCita>().HasData(
                new EstadoCita { IdEstadoCita = 1, Nombre = "Programada" },
                new EstadoCita { IdEstadoCita = 2, Nombre = "Atendida" },
                new EstadoCita { IdEstadoCita = 3, Nombre = "Cancelada" },
                new EstadoCita { IdEstadoCita = 4, Nombre = "Reprogramada" },
                new EstadoCita { IdEstadoCita = 5, Nombre = "No Asistio" }
            );

            modelBuilder.Entity<Especialidad>().HasData(
                new Especialidad { IdEspecialidad = 1, Nombre = "Medicina General", Descripcion = "Atencion general", Estado = true },
                new Especialidad { IdEspecialidad = 2, Nombre = "Pediatria", Descripcion = "Atencion de niños", Estado = true },
                new Especialidad { IdEspecialidad = 3, Nombre = "Obstetricia", Descripcion = "Control prenatal y salud materna", Estado = true },
                new Especialidad { IdEspecialidad = 4, Nombre = "Odontologia", Descripcion = "Atencion dental", Estado = true }
            );

            modelBuilder.Entity<Turno>().HasData(
                new Turno { IdTurno = 1, Nombre = "Mañana", HoraInicio = new TimeOnly(8, 0), HoraFin = new TimeOnly(12, 0), Estado = true },
                new Turno { IdTurno = 2, Nombre = "Tarde", HoraInicio = new TimeOnly(14, 0), HoraFin = new TimeOnly(18, 0), Estado = true }
            );
        }
    }
}
