using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;
using MeTube.Data.Entity;
using MeTube.Data.Repository;

namespace MeTube.Test.Repositories
{
    public class VideoRepositoryTests
    {
        private readonly Mock<IVideoRepository> _mockVideoRepo;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;

        public VideoRepositoryTests()
        {
            _mockVideoRepo = new Mock<IVideoRepository>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockUnitOfWork.Setup(uow => uow.Videos).Returns(_mockVideoRepo.Object);
        }
        [Fact]
        public async Task GetAllVideos_ShouldReturnListOfVideos()
        {
            // Arrange
            var videos = new List<Video>
            {
                new Video
                {
                    Id = 1,
                    UserId = 1,
                    Title = "164. What is the Future of Blazor?",
                    Description = "Should I learn a JavaScript framework or concentrate on mastering Blazor?",
                    Genre = "Programming"
                },
                new Video
                {
                    Id = 2,
                    UserId = 2,
                    Title = "string",
                    Description = "string",
                    Genre = "string"
                }
            };

        }
    }
}
