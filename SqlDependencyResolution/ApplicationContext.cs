using Microsoft.EntityFrameworkCore;

namespace SqlDependencyResolution
{
    public class ApplicationContext : DbContext
    {
        public DbSet<LogicTablePermission> LogicTablePermissions { get; set; }

        public ApplicationContext(DbContextOptions options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<LogicTablePermission>()
                .HasKey(nameof(LogicTablePermission.LogicNaturalKey), nameof(LogicTablePermission.TableName));

            modelBuilder.Entity<LogicTablePermission>()
                .Property(nameof(LogicTablePermission.LogicNaturalKey))
                .HasColumnName("LogicNK");

            modelBuilder.Entity<LogicTablePermission>()
                .Property(nameof(LogicTablePermission.TableName))
                .HasColumnName("TableNM");

            modelBuilder.Entity<LogicTablePermission>()
                .Property(nameof(LogicTablePermission.Permissions))
                .HasColumnName("Permissions");
        }
    }
}
