using MeTube.Data.Entity;
using Microsoft.EntityFrameworkCore;


namespace MeTube.Data
{
    public class ApplicationDbContext : DbContext
    {
        public virtual DbSet<User> Users { get; set; } = null!;
        public virtual DbSet<Video> Videos { get; set; }

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
                entity.HasKey(u => u.Id);
                entity.Property(u => u.Id).ValueGeneratedOnAdd();
                entity.Property(entity => entity.Username).IsRequired().HasMaxLength(20);
                entity.Property(entity => entity.Password).IsRequired().HasMaxLength(20);
                entity.Property(entity => entity.Email).IsRequired();
                entity.Property(entity => entity.Role).IsRequired().HasDefaultValue("User");

                // Relation with videos
                entity.HasMany(u => u.Videos).WithOne(v => v.User).HasForeignKey(v => v.UserId).OnDelete(DeleteBehavior.Cascade);
            });


            // Configure Video entity
            modelBuilder.Entity<Video>(entity =>
            {
                entity.HasKey(v => v.Id);
                entity.Property(v => v.Id).ValueGeneratedOnAdd();
                entity.Property(v => v.Title).IsRequired().HasMaxLength(30);
                entity.Property(v => v.Description).IsRequired().HasMaxLength(255);
                entity.Property(v => v.Genre).IsRequired().HasMaxLength(30);
                entity.Property(v => v.VideoUrl).HasMaxLength(2083); // Standard for URLs (length)
                entity.Property(v => v.ThumbnailUrl).HasMaxLength(2083); // Standard for URLs (length)
                entity.Property(v => v.BlobName).HasMaxLength(500);
                entity.Property(v => v.DateUploaded).IsRequired().HasColumnType("datetime");

                // Relation with user
                entity.HasOne(v => v.User).WithMany(u => u.Videos).HasForeignKey(v => v.UserId).IsRequired().OnDelete(DeleteBehavior.Cascade);
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
                Password = "pwd123",
                Email = "john.doe@example.com",
                Role = "Admin"
            };

            User user2 = new()
            {
                Id = 2,
                Username = "janedoe2",
                Password = "pwd456",
                Email = "jane.doe@example.com",
                Role = "User"
            };

            modelBuilder.Entity<User>().HasData(user1, user2);
        }
    }
}
