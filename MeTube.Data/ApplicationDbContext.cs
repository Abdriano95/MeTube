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
                entity.HasOne(l => l.User).WithMany(u => u.Likes).HasForeignKey(l => l.UserID).IsRequired().OnDelete(DeleteBehavior.ClientCascade);
                // Relation with video
                entity.HasOne(l => l.Video).WithMany(v => v.Likes).HasForeignKey(l => l.VideoID).IsRequired().OnDelete(DeleteBehavior.Cascade);
            });

            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Fixed date for seeding
            var baseDate = new DateTime(2024, 1, 1);

            // Seed Users
            var users = new List<User>
            {
                new User
                {
                    Id = 1,
                    Username = "admin",
                    Password = "adminpwd123",
                    Email = "admin@example.com",
                    Role = "Admin"
                },
                new User
                {
                    Id = 2,
                    Username = "john_doe",
                    Password = "password123",
                    Email = "john@example.com",
                    Role = "User"
                },
                new User
                {
                    Id = 3,
                    Username = "jane_smith",
                    Password = "password456",
                    Email = "jane@example.com",
                    Role = "User"
                },
                new User
                {
                    Id = 4,
                    Username = "tech_guru",
                    Password = "techpass789",
                    Email = "tech@example.com",
                    Role = "User"
                },
                new User
                {
                    Id = 5,
                    Username = "sports_fan",
                    Password = "sportspass789",
                    Email = "sports@example.com",
                    Role = "User"
                }
            };
            modelBuilder.Entity<User>().HasData(users);

            // Seed Videos
            var videos = new List<Video>
            {
                new Video
                {
                    Id = 1,
                    UserId = 1,
                    Title = "Learn Animation Basics",
                    Description = "Educational video about animation techniques",
                    Genre = "Education",
                    VideoUrl = "https://looplegionmetube20250129.blob.core.windows.net/videos/541bc83a-c39b-41e0-93e2-b353a5957e0b_faststart.mp4",
                    ThumbnailUrl = "https://looplegionmetube20250129.blob.core.windows.net/thumbnails/Big_Buck_Bunny_thumbnail_vlc.png",
                    BlobName = "541bc83a-c39b-41e0-93e2-b353a5957e0b_faststart.mp4",
                    DateUploaded = baseDate
                },
                new Video
                {
                    Id = 2,
                    UserId = 2,
                    Title = "Gaming Adventures",
                    Description = "Fun gaming moments with Big Buck Bunny",
                    Genre = "Gaming",
                    VideoUrl = "https://looplegionmetube20250129.blob.core.windows.net/videos/e1543acd-86de-46fb-994f-ee01dc9e4947_faststart.mp4",
                    ThumbnailUrl = "https://looplegionmetube20250129.blob.core.windows.net/thumbnails/Big_Buck_Bunny_thumbnail_vlc.png",
                    BlobName = "e1543acd-86de-46fb-994f-ee01dc9e4947_faststart.mp4",
                    DateUploaded = baseDate.AddDays(1)
                },
                new Video
                {
                    Id = 3,
                    UserId = 3,
                    Title = "Entertainment Weekly",
                    Description = "Weekly entertainment highlights",
                    Genre = "Entertainment",
                    VideoUrl = "https://looplegionmetube20250129.blob.core.windows.net/videos/da06b2df-5389-4f6b-9969-54b538c8062d_faststart.mp4",
                    ThumbnailUrl = "https://looplegionmetube20250129.blob.core.windows.net/thumbnails/Big_Buck_Bunny_thumbnail_vlc.png",
                    BlobName = "da06b2df-5389-4f6b-9969-54b538c8062d_faststart.mp4",
                    DateUploaded = baseDate.AddDays(2)
                },
                new Video
                {
                    Id = 4,
                    UserId = 4,
                    Title = "Tech News Today",
                    Description = "Latest in technology",
                    Genre = "Technology",
                    VideoUrl = "https://looplegionmetube20250129.blob.core.windows.net/videos/c1e82a0c-a48c-46aa-b33b-bf766ff77449_faststart.mp4",
                    ThumbnailUrl = "https://looplegionmetube20250129.blob.core.windows.net/thumbnails/Big_Buck_Bunny_thumbnail_vlc.png",
                    BlobName = "c1e82a0c-a48c-46aa-b33b-bf766ff77449_faststart.mp4",
                    DateUploaded = baseDate.AddDays(3)
                },
                new Video
                {
                    Id = 5,
                    UserId = 5,
                    Title = "Sports Highlights",
                    Description = "Best sports moments of the week",
                    Genre = "Sports",
                    VideoUrl = "https://looplegionmetube20250129.blob.core.windows.net/videos/a6e61589-a50a-482e-b3ec-5ccfb0ec04d6_faststart.mp4",
                    ThumbnailUrl = "https://looplegionmetube20250129.blob.core.windows.net/thumbnails/Big_Buck_Bunny_thumbnail_vlc.png",
                    BlobName = "a6e61589-a50a-482e-b3ec-5ccfb0ec04d6_faststart.mp4",
                    DateUploaded = baseDate.AddDays(4)
                },
                new Video
                {
                    Id = 6,
                    UserId = 1,
                    Title = "Music Session",
                    Description = "Amazing music compilation",
                    Genre = "Music",
                    VideoUrl = "https://looplegionmetube20250129.blob.core.windows.net/videos/fc30b0ae-c983-4af5-b9b6-6ec3a1b5609a_faststart.mp4",
                    ThumbnailUrl = "https://looplegionmetube20250129.blob.core.windows.net/thumbnails/Big_Buck_Bunny_thumbnail_vlc.png",
                    BlobName = "fc30b0ae-c983-4af5-b9b6-6ec3a1b5609a_faststart.mp4",
                    DateUploaded = baseDate.AddDays(5)
                },
                new Video
                {
                    Id = 7,
                    UserId = 2,
                    Title = "Breaking News",
                    Description = "Latest news updates",
                    Genre = "News",
                    VideoUrl = "https://looplegionmetube20250129.blob.core.windows.net/videos/b13bb460-5ec7-4e44-8f06-88c161aa10c6_faststart.mp4",
                    ThumbnailUrl = "https://looplegionmetube20250129.blob.core.windows.net/thumbnails/Big_Buck_Bunny_thumbnail_vlc.png",
                    BlobName = "b13bb460-5ec7-4e44-8f06-88c161aa10c6_faststart.mp4",
                    DateUploaded = baseDate.AddDays(6)
                },
                new Video
                {
                    Id = 8,
                    UserId = 3,
                    Title = "Miscellaneous Fun",
                    Description = "Random entertaining content",
                    Genre = "Other",
                    VideoUrl = "https://looplegionmetube20250129.blob.core.windows.net/videos/174c4367-4740-4166-a714-4774d113834d_faststart.mp4",
                    ThumbnailUrl = "https://looplegionmetube20250129.blob.core.windows.net/thumbnails/Big_Buck_Bunny_thumbnail_vlc.png",
                    BlobName = "174c4367-4740-4166-a714-4774d113834d_faststart.mp4",
                    DateUploaded = baseDate.AddDays(7)
                },
                new Video
                {
                    Id = 9,
                    UserId = 4,
                    Title = "Educational Tech",
                    Description = "Learning about technology",
                    Genre = "Education",
                    VideoUrl = "https://looplegionmetube20250129.blob.core.windows.net/videos/6495e892-4155-47e1-94b8-cbe317691e4f_faststart.mp4",
                    ThumbnailUrl = "https://looplegionmetube20250129.blob.core.windows.net/thumbnails/Big_Buck_Bunny_thumbnail_vlc.png",
                    BlobName = "6495e892-4155-47e1-94b8-cbe317691e4f_faststart.mp4",
                    DateUploaded = baseDate.AddDays(8)
                }
            };
            modelBuilder.Entity<Video>().HasData(videos);

            // Seed Comments
            var comments = new List<Comment>
            {
                new Comment
                {
                    Id = 1,
                    VideoId = 1,
                    UserId = 2,
                    Content = "Great educational content!",
                    DateAdded = baseDate.AddDays(1)
                },
                new Comment
                {
                    Id = 2,
                    VideoId = 1,
                    UserId = 3,
                    Content = "Very informative video",
                    DateAdded = baseDate.AddDays(1)
                },
                new Comment
                {
                    Id = 3,
                    VideoId = 2,
                    UserId = 1,
                    Content = "Amazing gaming content",
                    DateAdded = baseDate.AddDays(2)
                },
                new Comment
                {
                    Id = 4,
                    VideoId = 2,
                    UserId = 4,
                    Content = "Love this game!",
                    DateAdded = baseDate.AddDays(2)
                }
            };
            modelBuilder.Entity<Comment>().HasData(comments);

            // Seed Likes
            var likes = new List<Like>
            {
                new Like { VideoID = 1, UserID = 2 },
                new Like { VideoID = 1, UserID = 3 },
                new Like { VideoID = 1, UserID = 4 },
                new Like { VideoID = 2, UserID = 1 },
                new Like { VideoID = 2, UserID = 3 },
                new Like { VideoID = 3, UserID = 1 },
                new Like { VideoID = 3, UserID = 2 },
                new Like { VideoID = 4, UserID = 5 }
            };
            modelBuilder.Entity<Like>().HasData(likes);

            // Seed History
            var histories = new List<History>
            {
                new History
                {
                    Id = 1,
                    VideoId = 1,
                    UserId = 2,
                    DateWatched = baseDate.AddDays(1)
                },
                new History
                {
                    Id = 2,
                    VideoId = 1,
                    UserId = 3,
                    DateWatched = baseDate.AddDays(1)
                },
                new History
                {
                    Id = 3,
                    VideoId = 2,
                    UserId = 1,
                    DateWatched = baseDate.AddDays(2)
                },
                new History
                {
                    Id = 4,
                    VideoId = 2,
                    UserId = 4,
                    DateWatched = baseDate.AddDays(2)
                }
            };
            modelBuilder.Entity<History>().HasData(histories);
        }
    }
}
