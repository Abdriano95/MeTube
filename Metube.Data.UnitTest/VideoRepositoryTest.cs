using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MeTube.Data;
using MeTube.Data.Entity;
using MeTube.Data.Repository;

namespace Metube.Data.UnitTest
{
    public class VideoRepositoryTest
    {
        private readonly List<Video> _videos;
        private readonly Video _video1;
        private readonly Video _video2;
        private readonly Mock<ApplicationDbContext> _mockDbContext;
        private readonly Mock<DbSet<Video>> _mockVideoSet;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IVideoRepository> _mockVideoRepository;
        private readonly VideoRepository _videoRepository;

        public VideoRepositoryTest()
        {
            // Testdata
            _videos = new()
            {
                new Video
                {
                    Id = 1,
                    Title = "164. What is the Future of Blazor?",
                    Description = "Should I learn a JavaScript framework or concentrate on Blazor?",
                    Genre = "Programming",
                    UserId = 1,
                    VideoUrl = "https://looplegionmetube20250129.blob...",
                    ThumbnailUrl = "https://looplegionmetube20250129.blob...",
                    DateUploaded = new DateTime(2025, 02, 01)
                },
                new Video
                {
                    Id = 3,
                    Title = "fdfdfd",
                    Description = "sfssssssssssssssssssssssssssssssssss",
                    Genre = "Music",
                    UserId = 1,
                    VideoUrl = "https://looplegionmetube20250129.blob...",
                    ThumbnailUrl = "https://upload.wikimedia.org/...",
                    DateUploaded = new DateTime(2025, 02, 04)
                }
            };

            _video1 = _videos[0];
            _video2 = _videos[1];

            // mmocka DbSet och ApplicationDbContext
            _mockVideoSet = new Mock<DbSet<Video>>();
            _mockDbContext = new Mock<ApplicationDbContext>();

            // Ställer in DbSet-mock så att den fungerar med testdatan..
            _mockVideoSet.As<IQueryable<Video>>().Setup(m => m.Provider).Returns(_videos.AsQueryable().Provider);
            _mockVideoSet.As<IQueryable<Video>>().Setup(m => m.Expression).Returns(_videos.AsQueryable().Expression);
            _mockVideoSet.As<IQueryable<Video>>().Setup(m => m.ElementType).Returns(_videos.AsQueryable().ElementType);
            _mockVideoSet.As<IQueryable<Video>>().Setup(m => m.GetEnumerator()).Returns(_videos.AsQueryable().GetEnumerator());

            // Mocka ApplicationDbContext för att returnera den mockade DbSet
            _mockDbContext.Setup(c => c.Videos).Returns(_mockVideoSet.Object);

            // mockar ideoRepository och UnitOfWork
            _mockVideoRepository = new Mock<IVideoRepository>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _videoRepository = new VideoRepository(_mockDbContext.Object);
        }

        [Fact]
        public async Task GetAllVideosAsync_ShouldReturnAllVideos()
        {
            // Arrange
            _mockVideoRepository.Setup(repo => repo.GetAllVideosAsync()).ReturnsAsync(_videos);

            // Act
            var result = await _videoRepository.GetAllVideosAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().Contain(_videos);
        }

        [Fact]
        public async Task GetVideoByIdAsync_ExistingId_ShouldReturnVideo()
        {
            // Arrange
            int existingId = 1;
            _mockVideoRepository.Setup(repo => repo.GetVideoByIdAsync(existingId)).ReturnsAsync(_video1);

            // Act
            var result = await _videoRepository.GetVideoByIdAsync(existingId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(_video1);
        }

        [Fact]
        public async Task AddVideoAsync_ShouldAddVideo()
        {
            // Arrange
            var newVideo = new Video
            {
                Id = 5,
                Title = "New Video",
                Description = "New Description",
                Genre = "Action",
                UserId = 1, // Använder en befintlig användare från databasen
                VideoUrl = "https://example.com/newvideo.mp4",
                ThumbnailUrl = "https://example.com/newthumbnail.jpg",
                DateUploaded = DateTime.UtcNow
            };

            _mockVideoRepository.Setup(repo => repo.AddVideoAsync(newVideo)).Returns(Task.CompletedTask);

            // Act
            await _videoRepository.AddVideoAsync(newVideo);

            // Assert
            _mockVideoRepository.Verify(repo => repo.AddVideoAsync(newVideo), Times.Once);
        }
    }
}
