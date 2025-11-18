using Microsoft.EntityFrameworkCore;
using ST10439055_POE_PROG6212.Models;

namespace ST10439055_POE_PROG6212.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Lecturer> Lecturers { get; set; }
        public DbSet<Claim> Claims { get; set; }
        public DbSet<SupportingDocument> SupportingDocuments { get; set; }
        public DbSet<Approval> Approvals { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Lecturer>(entity =>
            {
                entity.Property(l => l.IsActive)
                    .HasDefaultValue(true);
                entity.Property(l => l.Role)
                    .HasDefaultValue(UserRole.Lecturer);
            });
        }
    }
}
