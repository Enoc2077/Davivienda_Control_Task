using Davivienda.Migrations.ModelosEF;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Davivienda.Migrations.DbContext
{
    // 1. LA FÁBRICA: Solo sirve para que la consola de 'dotnet ef' pueda crear el contexto
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

            // Usamos tu servidor real: DESKTOP-IJ2LO3K\SQLEXPRESS
            optionsBuilder.UseSqlServer("Server=DESKTOP-IJ2LO3K\\SQLEXPRESS;Database=Davivienda_Asignaciones;Trusted_Connection=True;TrustServerCertificate=True;");

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }

    // 2. EL CONTEXTO: Aquí es donde definas tus tablas y mapeos
    public class ApplicationDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Definición de las tablas (DbSets)
        public DbSet<Area> Areas { get; set; }
        public DbSet<Roles> Roles { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Proyecto> Proyectos { get; set; }
        public DbSet<DetalleProyecto> DetallesProyectos { get; set; }
        public DbSet<Procesos> Procesos { get; set; }
        public DbSet<Prioridad> Prioridades { get; set; }
        public DbSet<Tarea> Tareas { get; set; }
        public DbSet<Friccion> Fricciones { get; set; }
        public DbSet<Soluciones> Soluciones { get; set; }
        public DbSet<Comentarios> Comentarios { get; set; }
        public DbSet<Documentacion> Documentaciones { get; set; }
        public DbSet<BitacoraSoluciones> BitacoraSoluciones { get; set; }
        public DbSet<BitacoraFriccion> BitacoraFricciones { get; set; }
        public DbSet<Notificaciones> Notificaciones { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Mapeo específico para AREA
            modelBuilder.Entity<Area>(entity =>
            {
                entity.ToTable("AREA"); 
                entity.HasKey(e => e.ARE_ID);
                entity.Property(e => e.ARE_FEC_MOD).HasColumnName("ARE_FEC_MOD"); 
            });


            modelBuilder.Entity<Roles>(entity => //mapeo tabla roles
            {
                entity.ToTable("ROLES");
                entity.HasKey(e => e.ROL_ID);

                entity.Property(e => e.ROL_FEC_CRE)
                    .HasColumnName("ROL_FEC_CRE")
                    .IsRequired()
                    .HasDefaultValueSql("GETDATE()"); 

                entity.Property(e => e.ROL_FEC_MOD)
                    .HasColumnName("ROL_FEC_MOD")
                    .IsRequired()
                    .HasDefaultValueSql("GETDATE()");
            });


            modelBuilder.Entity<Tarea>(entity =>
            {
                entity.ToTable("TAREA");
                entity.HasKey(e => e.TAR_ID);

                // Ampliamos TAR_DES a MAX para que soporte el string de Base64 sin cortes
                entity.Property(e => e.TAR_DES)
                      .HasColumnName("TAR_DES")
                      .HasColumnType("nvarchar(max)");
            });



            // Mapeo USUARIO (Asegura que las llaves foráneas funcionen)
            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.ToTable("USUARIO");
                entity.HasKey(e => e.USU_ID);

                // Solo pones el área porque crees que el rol ya "se sabe"
                entity.HasOne(u => u.Area)
                      .WithMany()
                      .HasForeignKey(u => u.ARE_ID);
            });

            modelBuilder.Entity<BitacoraSoluciones>(entity =>
            {
                entity.ToTable("BITACORA_SOLUCIONES");
                entity.HasKey(e => e.BIT_SOL_ID);

                // Configuramos explícitamente el tipo de columna como datetimeoffset
                entity.Property(e => e.BIT_SOL_TIE_TOT_TRA)
                      .HasColumnName("BIT_SOL_TIE_TOT_TRA")
                      .HasColumnType("datetimeoffset");
            });


            // Mapeo de las demás tablas en mayúsculas
            modelBuilder.Entity<Roles>().ToTable("ROLES");
            modelBuilder.Entity<Usuario>().ToTable("USUARIO");
            modelBuilder.Entity<Proyecto>().ToTable("PROYECTO");
            modelBuilder.Entity<DetalleProyecto>().ToTable("DETALLES_PROYECTO");
            modelBuilder.Entity<Procesos>().ToTable("PROCESO");
            modelBuilder.Entity<Prioridad>().ToTable("PRIORIDAD");
            modelBuilder.Entity<Tarea>().ToTable("TAREA");
            modelBuilder.Entity<Friccion>().ToTable("FRICCION");
            modelBuilder.Entity<Soluciones>().ToTable("SOLUCIONES");
            modelBuilder.Entity<Comentarios>().ToTable("COMENTARIOS");
            modelBuilder.Entity<Documentacion>().ToTable("DOCUMENTACION");
            modelBuilder.Entity<BitacoraSoluciones>().ToTable("BITACORA_SOLUCIONES");
            modelBuilder.Entity<BitacoraFriccion>().ToTable("BITACORA_FRICCIONES");
            modelBuilder.Entity<Notificaciones>().ToTable("NOTIFICACIONES");
        }
    }
}