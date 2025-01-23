using MeTube.Data.Entity;
using Microsoft.EntityFrameworkCore;


namespace MeTube.Data
{
    public class ApplicationDbContext : DbContext
    {
        public virtual DbSet<User> Users { get; set; } = null!;
        public virtual DbSet<Admin> Admins { get; set; } = null!;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(entity => entity.Username).IsRequired().HasMaxLength(20);
                entity.Property(entity => entity.Password).IsRequired().HasMaxLength(20);
                entity.Property(entity => entity.Email).IsRequired();
            });

            // Configure Admin entity

            modelBuilder.Entity<Admin>(entity =>
            {
                entity.HasBaseType<User>();
            });

            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed Users
            User user1 = new()
            {
                Id = 1,
                Username = "johndoe1",
                Password = "john.doe@example.com",
                Email = "pwd123"
            };

            Admin admin1 = new()
            {
                Id = 2,
                Username = "janesmith2",
                Password = "pwd456",
                Email = "jane.smith@example.com",
            };

            modelBuilder.Entity<User>().HasData(user1);
            modelBuilder.Entity<Admin>().HasData(admin1);
        }
    }
}
