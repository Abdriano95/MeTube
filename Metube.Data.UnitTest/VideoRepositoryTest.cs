using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MeTube.Data;
using MeTube.Data.Entity;
using MeTube.Data.Repository;

namespace MeTube.Data.UnitTest
{
    public class VideoRepositoryTest
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly VideoRepository _videoRepository;

        public VideoRepositoryTest()
        {
            // Skapa en in-memory databas för tester
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _dbContext = new ApplicationDbContext(options);
            _videoRepository = new VideoRepository(_dbContext);

            // Seeda testdata i databasen
            _dbContext.Videos.AddRange(new List<Video>
            {
                new Video { Id = 1, UserId = 1, Title = "164. What is the Future of Blazor?", Description = "Should I learn a JavaScript framework?", Genre = "Programming" },
                new Video { Id = 2, UserId = 2, Title = "string", Description = "string", Genre = "string" }
            });

            _dbContext.SaveChanges();
        }

        [Fact]
        public async Task AddVideoAsync_ShouldAddVideo()
        {
            // Hämta en befintlig video från databasen för att återanvända Genre och Description
            var existingVideo = await _dbContext.Videos.FirstOrDefaultAsync(v => v.Id == 1);

            var newVideo = new Video
            {
                Id = 3,
                UserId = 1,
                Title = "New Video",
                Description = existingVideo?.Description ?? "Default Description",
                Genre = existingVideo?.Genre ?? "Default Genre"
            };

            // Act - Lägg till videon
            await _videoRepository.AddVideoAsync(newVideo);
            await _dbContext.SaveChangesAsync();

            // Assert - Kontrollera att videon finns i databasen
            var addedVideo = await _dbContext.Videos.FindAsync(3);
            addedVideo.Should().NotBeNull();
            addedVideo.Title.Should().Be("New Video");
            addedVideo.Genre.Should().Be(existingVideo?.Genre);
            addedVideo.Description.Should().Be(existingVideo?.Description);
        }

        [Fact]
        public async Task GetAllVideosAsync_ShouldReturnAllVideos()
        {
            // Act
            var videos = await _videoRepository.GetAllVideosAsync();

            // Assert
            videos.Should().NotBeNull();
            videos.Should().HaveCount(2); // för at vi sseedade  2 videos 
        }

        [Fact]
        public async Task GetVideoByIdAsync_ExistingId_ShouldReturnVideo()
        {
            // Arrange
            var existingId = 1;

            // Act
            var video = await _videoRepository.GetVideoByIdAsync(existingId);

            // Assert
            video.Should().NotBeNull();
            video.Id.Should().Be(existingId);
            video.Title.Should().Be("164. What is the Future of Blazor?");
        }

        [Fact]
        public async Task DeleteVideoAsync_ShouldDeleteVideo()
        {
            // Arrange - Hämta en video som ska raderas
            var existingVideo = await _dbContext.Videos.FirstOrDefaultAsync(v => v.Id == 1);
            existingVideo.Should().NotBeNull(); // Se till att den finns innan vi tar bort den

            // Act - Ta bort videon
            await _videoRepository.DeleteVideo(existingVideo);
            await _dbContext.SaveChangesAsync();

            // Assert - Kontrollera att den INTE längre finns i databasen
            var deletedVideo = await _dbContext.Videos.FindAsync(1);
            deletedVideo.Should().BeNull(); 
        }
    }
}
