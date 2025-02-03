using MeTube.Data.Entity;
using Microsoft.EntityFrameworkCore;


namespace MeTube.Data
{
    public class ApplicationDbContext : DbContext
    {
        public virtual DbSet<User> Users { get; set; } = null!;
        public virtual DbSet<Video> Videos { get; set; } = null!;
        public virtual DbSet<History> Histories { get; set; } = null!;
        public virtual DbSet<Comment> Comments { get; set; } = null!;
        public virtual DbSet<Like> Likes { get; set; } = null!;




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
                entity.HasMany(u => u.Videos).WithOne(v => v.User).HasForeignKey(v => v.UserId).OnDelete(DeleteBehavior.Restrict);
                // Relation with histories
                entity.HasMany(u => u.Histories).WithOne(h => h.User).HasForeignKey(h => h.UserId).OnDelete(DeleteBehavior.Restrict);
                // Relation with comments
                entity.HasMany(u => u.Comments).WithOne(c => c.User).HasForeignKey(c => c.UserId).OnDelete(DeleteBehavior.Restrict);
                // Relation with likes
                entity.HasMany(u => u.Likes).WithOne(l => l.User).HasForeignKey(l => l.UserID).OnDelete(DeleteBehavior.Restrict);
            });


            // Configure Video entity
            modelBuilder.Entity<Video>(entity =>
            {
                entity.HasKey(v => v.Id);
                entity.Property(v => v.Id).ValueGeneratedOnAdd();
                entity.Property(v => v.Title).IsRequired().HasMaxLength(120);
                entity.Property(v => v.Description).IsRequired().HasMaxLength(255);
                entity.Property(v => v.Genre).IsRequired().HasMaxLength(30);
                entity.Property(v => v.VideoUrl).HasMaxLength(2083); // Standard for URLs (length)
                entity.Property(v => v.ThumbnailUrl).HasMaxLength(2083); // Standard for URLs (length)
                entity.Property(v => v.BlobName).HasMaxLength(500);
                entity.Property(v => v.DateUploaded).IsRequired().HasColumnType("datetime");

                // Relation with user
                entity.HasOne(v => v.User).WithMany(u => u.Videos).HasForeignKey(v => v.UserId).IsRequired().OnDelete(DeleteBehavior.Restrict);
                // Relation with comments
                entity.HasMany(v => v.Comments).WithOne(c => c.Video).HasForeignKey(c => c.VideoId).OnDelete(DeleteBehavior.Restrict);
                // Relation with likes
                entity.HasMany(v => v.Likes).WithOne(l => l.Video).HasForeignKey(l => l.VideoID).OnDelete(DeleteBehavior.Restrict);
                // Relation with history
                entity.HasMany(v => v.Histories).WithOne(h => h.Video).HasForeignKey(h => h.VideoId).OnDelete(DeleteBehavior.Restrict);
            });

            // Configure History entity
            modelBuilder.Entity<History>(entity =>
            {
                entity.HasKey(h => h.Id);
                entity.Property(h => h.Id).ValueGeneratedOnAdd();
                entity.Property(h => h.DateWatched).IsRequired().HasColumnType("datetime");
                // Relation with user
                entity.HasOne(h => h.User).WithMany(u => u.Histories).HasForeignKey(h => h.UserId).IsRequired().OnDelete(DeleteBehavior.Cascade);
                // Relation with video
                entity.HasOne(h => h.Video).WithMany(v => v.Histories).HasForeignKey(h => h.VideoId).IsRequired().OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Comment entity
            modelBuilder.Entity<Comment>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Id).ValueGeneratedOnAdd();
                entity.Property(c => c.Content).IsRequired().HasMaxLength(255);
                entity.Property(c => c.DateAdded).IsRequired().HasColumnType("datetime");
                // Relation with user
                entity.HasOne(c => c.User).WithMany(u => u.Comments).HasForeignKey(c => c.UserId).IsRequired().OnDelete(DeleteBehavior.Cascade);
                // Relation with video
                entity.HasOne(c => c.Video).WithMany(v => v.Comments).HasForeignKey(c => c.VideoId).IsRequired().OnDelete(DeleteBehavior.Cascade);
            });


            // Configure Like entity
            modelBuilder.Entity<Like>(entity =>
            {
                entity.HasKey(l => new { l.VideoID, l.UserID });

                // Relation with user
                entity.HasOne(l => l.User).WithMany(u => u.Likes).HasForeignKey(l => l.UserID).IsRequired().OnDelete(DeleteBehavior.Cascade);
                // Relation with video
                entity.HasOne(l => l.Video).WithMany(v => v.Likes).HasForeignKey(l => l.VideoID).IsRequired().OnDelete(DeleteBehavior.Cascade);
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

            // Seed Videos
            Video video1 = new()
            {
                Id = 1,
                UserId = 1,
                Title = "164. What is the Future of Blazor? Should I Learn Blazor?",
                Description = "Should I learn a JavaScript framework or concentrate on mastering Blazor? What is the future of Blazor? Is Microsoft invested in making Blazor great? We will answer these questions in today's Dev Questions episode.   Website: https://www.iamtimcorey.com/",
                Genre = "Programming",
                VideoUrl = "https://looplegionmetube20250129.blob.core.windows.net/videos/youtube_OUUlO8fQOfE_1920x1080_h264.mp4",
                ThumbnailUrl = "https://looplegionmetube20250129.blob.core.windows.net/thumbnails/whatisthefutureofblazor.jpg",
                BlobName = "youtube_OUUlO8fQOfE_1920x1080_h264.mp4",
                //2025-02-01 13:48:10
                DateUploaded = new DateTime(2025, 2, 1) 
            };

            Video video2 = new()
            {
                Id = 2,
                UserId = 2,
                Title = "string",
                Description = "string",
                Genre = "string",
                VideoUrl = "https://looplegionmetube20250129.blob.core.windows.net/videos/videoplayback%20%281%29.mp4",
                ThumbnailUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/e/e8/YouTube_Diamond_Play_Button.png/1200px-YouTube_Diamond_Play_Button.png",
                BlobName = "videoplayback (1).mp4",
                DateUploaded = new DateTime(2025, 2, 1)
            };

            modelBuilder.Entity<Video>().HasData(video1, video2);
        }
    }
}
