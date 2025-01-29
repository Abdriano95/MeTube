using MeTube.Data.Entity;
using Microsoft.EntityFrameworkCore;


namespace MeTube.Data
{
    public class ApplicationDbContext : DbContext
    {
        public virtual DbSet<User> Users { get; set; } = null!;
        public virtual DbSet<Admin> Admins { get; set; } = null!;
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

            // Configure Video entity
            modelBuilder.Entity<Video>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.Title).IsRequired().HasMaxLength(30);
                entity.Property(e => e.Description).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Genre).IsRequired().HasMaxLength(30);
                entity.Property(e => e.VideoUrl).HasMaxLength(2083); // Standard for URLs (length)
                entity.Property(e => e.DateUploaded).IsRequired().HasColumnType("datetime");

                // Relation with user
                entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
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
                Email = "john.doe@example.com"
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

            // Seed Videos
            Video video1 = new()
            {
                Id = 1,
                UserId = 1,
                Title = "Learning C# Basics",
                Description = "An introduction to C# programming for beginners.",
                Genre = "Education",
                VideoUrl = "https://example.com/video1",
                DateUploaded = new DateTime(2023, 1, 1)
            };

            Video video2 = new()
            {
                Id = 2,
                UserId = 1,
                Title = "Advanced LINQ Tips",
                Description = "Learn advanced LINQ techniques to simplify your C# code.",
                Genre = "Tech",
                VideoUrl = "https://example.com/video2",
                DateUploaded = new DateTime(2023, 1, 1)
            };

            Video video3 = new()
            {
                Id = 3,
                UserId = 2,
                Title = "Introduction to .NET MAUI",
                Description = "Explore .NET MAUI and learn how to build cross-platform apps.",
                Genre = "Education",
                VideoUrl = "https://example.com/video3",
                DateUploaded = new DateTime(2023, 1, 1)
            };

            modelBuilder.Entity<Video>().HasData(video1, video2, video3);
        }
    }
}
