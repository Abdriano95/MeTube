using System;
using System.Collections.Generic;
using System.Linq;
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

            // När SaveChangesAsync() anropas, returnera "1" för att simulera 
            // att 1 förändring har sparats i databasen.
            _mockUnitOfWork
                .Setup(uow => uow.SaveChangesAsync())
                .ReturnsAsync(1);

            // UnitOfWork.Videos ska returnera vår mockade VideoRepository.
            _mockUnitOfWork
                .Setup(uow => uow.Videos)
                .Returns(_mockVideoRepo.Object);
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
                    Description = "Should I learn a JavaScript framework...",
                    Genre = "Programming"
                },
                new Video
                {
                    Id = 2,
                    UserId = 2,
                    Title = "A simple test video",
                    Description = "Something about unit testing",
                    Genre = "Testing"
                }
            };

            _mockVideoRepo
                .Setup(repo => repo.GetAllVideosAsync())
                .ReturnsAsync(videos);

            // Act
            var result = await _mockVideoRepo.Object.GetAllVideosAsync();

            // Assert
            result.Should().NotBeNull().And.HaveCount(2);
            result.First().Id.Should().Be(1);

            _mockVideoRepo.Verify(repo => repo.GetAllVideosAsync(), Times.Once);
        }

      
        [Fact]
        public async Task GetVideoByIdAsync_ExistingVideo_ShouldReturnVideo()
        {
            // Arrange
            var video = new Video
            {
                Id = 1,
                UserId = 1,
                Title = "Existing Video",
                Description = "Test Description",
                Genre = "Test Genre"
            };

            _mockVideoRepo
                .Setup(repo => repo.GetVideoByIdAsync(1))
                .ReturnsAsync(video);

            // Act
            var result = await _mockVideoRepo.Object.GetVideoByIdAsync(1);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(1);
            result.Title.Should().Be("Existing Video");

            _mockVideoRepo.Verify(repo => repo.GetVideoByIdAsync(1), Times.Once);
        }

        [Fact]
        public async Task GetVideoByIdAsync_NonExistingVideo_ShouldReturnNull()
        {
            // Arrange
            _mockVideoRepo
                .Setup(repo => repo.GetVideoByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((Video?)null);

            // Act
            var result = await _mockVideoRepo.Object.GetVideoByIdAsync(999);

            // Assert
            result.Should().BeNull();
            _mockVideoRepo.Verify(repo => repo.GetVideoByIdAsync(999), Times.Once);
        }

       
        [Fact]
        public async Task GetVideoByIdWithTrackingAsync_ExistingVideo_ShouldReturnVideo()
        {
            // Arrange
            var video = new Video
            {
                Id = 1,
                UserId = 1,
                Title = "Tracked Video",
                Description = "Some description",
                Genre = "Programming"
            };

            _mockVideoRepo
                .Setup(repo => repo.GetVideoByIdWithTrackingAsync(1))
                .ReturnsAsync(video);

            // Act
            var result = await _mockVideoRepo.Object.GetVideoByIdWithTrackingAsync(1);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(1);
            result.Title.Should().Be("Tracked Video");

            _mockVideoRepo.Verify(repo => repo.GetVideoByIdWithTrackingAsync(1), Times.Once);
        }

        [Fact]
        public async Task GetVideoByIdWithTrackingAsync_NonExistingVideo_ShouldReturnNull()
        {
            // Arrange
            _mockVideoRepo
                .Setup(repo => repo.GetVideoByIdWithTrackingAsync(It.IsAny<int>()))
                .ReturnsAsync((Video?)null);

            // Act
            var result = await _mockVideoRepo.Object.GetVideoByIdWithTrackingAsync(999);

            // Assert
            result.Should().BeNull();
            _mockVideoRepo.Verify(repo => repo.GetVideoByIdWithTrackingAsync(999), Times.Once);
        }

       
        [Fact]
        public async Task GetVideosByUserIdAsync_ValidUser_ShouldReturnVideos()
        {
            // Arrange
            var userId = 1;
            var videos = new List<Video>
            {
                new Video
                {
                    Id = 10,
                    UserId = userId,
                    Title = "User1 Video1",
                    Description = "Desc #1",
                    Genre = "Programming"
                },
                new Video
                {
                    Id = 11,
                    UserId = userId,
                    Title = "User1 Video2",
                    Description = "Desc #2",
                    Genre = "Programming"
                }
            };

            _mockVideoRepo
                .Setup(repo => repo.GetVideosByUserIdAsync(userId))
                .ReturnsAsync(videos);

            // Act
            var result = await _mockVideoRepo.Object.GetVideosByUserIdAsync(userId);

            // Assert
            result.Should().NotBeNull().And.HaveCount(2);
            result.First().UserId.Should().Be(userId);

            _mockVideoRepo.Verify(repo => repo.GetVideosByUserIdAsync(userId), Times.Once);
        }

        [Fact]
        public async Task GetVideosByUserIdAsync_NoVideosFound_ShouldReturnEmptyList()
        {
            // Arrange
            _mockVideoRepo
                .Setup(repo => repo.GetVideosByUserIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new List<Video>()); // Tom lista

            // Act
            var result = await _mockVideoRepo.Object.GetVideosByUserIdAsync(999);

            // Assert
            result.Should().NotBeNull().And.BeEmpty();
            _mockVideoRepo.Verify(repo => repo.GetVideosByUserIdAsync(999), Times.Once);
        }

      
        [Fact]
        public async Task AddVideoAsync_ValidVideo_ShouldAddAndSave()
        {
            // Arrange
            var newVideo = new Video
            {
                Id = 1,
                UserId = 1,
                Title = "New Video",
                Description = "Some Description",
                Genre = "Some Genre"
            };

            _mockVideoRepo
                .Setup(repo => repo.AddVideoAsync(newVideo))
                .Returns(Task.CompletedTask);

            // Act
            await _mockVideoRepo.Object.AddVideoAsync(newVideo);
            var saveResult = await _mockUnitOfWork.Object.SaveChangesAsync();

            // Assert
            saveResult.Should().Be(1);
            _mockVideoRepo.Verify(repo => repo.AddVideoAsync(newVideo), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task AddVideoAsync_NullVideo_ShouldThrowException()
        {
            // Arrange
            _mockVideoRepo
                .Setup(repo => repo.AddVideoAsync(null!))
                .ThrowsAsync(new ArgumentNullException());

            // Act
            Func<Task> act = async () => await _mockVideoRepo.Object.AddVideoAsync(null!);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>();
            _mockVideoRepo.Verify(repo => repo.AddVideoAsync(null!), Times.Once);
        }

       
        [Fact]
        public async Task AddVideoWithoutSaveAsync_ValidVideo_ShouldAddWithoutCallingSaveHere()
        {
            // Arrange
            var video = new Video
            {
                Id = 2,
                UserId = 2,
                Title = "Video Without Immediate Save",
                Description = "Test desc",
                Genre = "Test genre"
            };

            _mockVideoRepo
                .Setup(repo => repo.AddVideoWithoutSaveAsync(video))
                .Returns(Task.CompletedTask);

            // Act
            await _mockVideoRepo.Object.AddVideoWithoutSaveAsync(video);
            // Vi anropar INTE SaveChangesAsync() här

            // Assert
            _mockVideoRepo.Verify(repo => repo.AddVideoWithoutSaveAsync(video), Times.Once);
        }

       
        [Fact]
        public async Task UpdateVideo_ValidVideo_ShouldUpdateAndSave()
        {
            // Arrange
            var video = new Video
            {
                Id = 1,
                UserId = 1,
                Title = "Original Title",
                Description = "Original Description",
                Genre = "Original Genre"
            };

            _mockVideoRepo.Setup(repo => repo.UpdateVideo(video));

            // Act
            _mockVideoRepo.Object.UpdateVideo(video);
            var saveResult = await _mockUnitOfWork.Object.SaveChangesAsync();

            // Assert
            saveResult.Should().Be(1);
            _mockVideoRepo.Verify(repo => repo.UpdateVideo(video), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public void UpdateVideo_NullVideo_ShouldThrowException()
        {
            // Arrange
            _mockVideoRepo
                .Setup(repo => repo.UpdateVideo(null!))
                .Throws<ArgumentNullException>();

            // Act
            Action act = () => _mockVideoRepo.Object.UpdateVideo(null!);

            // Assert
            act.Should().Throw<ArgumentNullException>();
            _mockVideoRepo.Verify(repo => repo.UpdateVideo(null!), Times.Once);
        }

       
        [Fact]
        public async Task DeleteVideo_ValidVideo_ShouldDeleteAndSave()
        {
            // Arrange
            var video = new Video
            {
                Id = 1,
                UserId = 1,
                Title = "To be deleted",
                Description = "Delete desc",
                Genre = "Delete genre"
            };

            _mockVideoRepo
                .Setup(repo => repo.DeleteVideo(video))
                .Returns(Task.CompletedTask);

            // Act
            await _mockVideoRepo.Object.DeleteVideo(video);
            var saveResult = await _mockUnitOfWork.Object.SaveChangesAsync();

            // Assert
            saveResult.Should().Be(1);
            _mockVideoRepo.Verify(repo => repo.DeleteVideo(video), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteVideo_NonExistingVideo_ShouldThrowException()
        {
            // Arrange
            var nonExistentVideo = new Video
            {
                Id = 999,
                UserId = 99,
                Title = "Non existent",
                Description = "Desc",
                Genre = "Genre"
            };

            _mockVideoRepo
                .Setup(repo => repo.DeleteVideo(nonExistentVideo))
                .ThrowsAsync(new ArgumentException("Video does not exist."));

            // Act
            Func<Task> act = async () => await _mockVideoRepo.Object.DeleteVideo(nonExistentVideo);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                     .WithMessage("Video does not exist.*");
            _mockVideoRepo.Verify(repo => repo.DeleteVideo(nonExistentVideo), Times.Once);
        }

       
        [Fact]
        public async Task GetVideoUploaderUsernameAsync_ValidId_ShouldReturnUsername()
        {
            // Arrange
            var videoId = 1;
            _mockVideoRepo
                .Setup(repo => repo.GetVideoUploaderUsernameAsync(videoId))
                .ReturnsAsync("TestUser");

            // Act
            var username = await _mockVideoRepo.Object.GetVideoUploaderUsernameAsync(videoId);

            // Assert
            username.Should().Be("TestUser");
            _mockVideoRepo.Verify(repo => repo.GetVideoUploaderUsernameAsync(videoId), Times.Once);
        }

        [Fact]
        public async Task GetVideoUploaderUsernameAsync_NonExistingId_ShouldReturnNullOrEmpty()
        {
            // Arrange
            _mockVideoRepo
                .Setup(repo => repo.GetVideoUploaderUsernameAsync(It.IsAny<int>()))
                .ReturnsAsync(string.Empty);

            // Act
            var username = await _mockVideoRepo.Object.GetVideoUploaderUsernameAsync(999);

            // Assert
            username.Should().BeEmpty();
            _mockVideoRepo.Verify(repo => repo.GetVideoUploaderUsernameAsync(999), Times.Once);
        }
    }
}
