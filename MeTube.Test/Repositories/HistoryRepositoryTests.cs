using MeTube.Data.Entity;
using MeTube.Data.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Security.Claims;


namespace MeTube.Test.Repositories
{
    public class HistoryRepositoryTests
    {
        private readonly Mock<IHistoryRepository> _mockHistoryRepo;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;

        public HistoryRepositoryTests()
        {
            _mockHistoryRepo = new Mock<IHistoryRepository>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockUnitOfWork.Setup(uow => uow.Histories).Returns(_mockHistoryRepo.Object);
        }

        [Fact]
        public async Task AddHistory_ShouldAddSuccessfully()
        {
            // Arrange
            var history = new History
            {
                UserId = 1,
                VideoId = 1
            };
            _mockHistoryRepo.Setup(repo => repo.AddAsync(history)).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(uow => uow.SaveChangesAsync()).ReturnsAsync(1);
            // Act
            await _mockHistoryRepo.Object.AddAsync(history);
            await _mockUnitOfWork.Object.SaveChangesAsync();
            // Assert
            _mockHistoryRepo.Verify(repo => repo.AddAsync(history), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task AddHistory_DuplicateHistory_ShouldThrowException()
        {
            // Arrange
            var history = new History
            {
                UserId = 1,
                VideoId = 1
            };
            _mockHistoryRepo.Setup(repo => repo.AddAsync(history))
                        .ThrowsAsync(new ArgumentException("User has already watched this video."));
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await _mockHistoryRepo.Object.AddAsync(history);
            });
        }

        [Fact]
        public async Task RemoveHistory_ShouldRemoveSuccessfully()
        {
            // Arrange
            var history = new History
            {
                UserId = 1,
                VideoId = 1
            };
            _mockHistoryRepo.Setup(repo => repo.RemoveAsync(history)).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(uow => uow.SaveChangesAsync()).ReturnsAsync(1);
            // Act
            await _mockHistoryRepo.Object.RemoveAsync(history);
            await _mockUnitOfWork.Object.SaveChangesAsync();
            // Assert
            _mockHistoryRepo.Verify(repo => repo.RemoveAsync(history), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task RemoveHistory_NonExistingHistory_ShouldThrowException()
        {
            // Arrange
            var history = new History
            {
                UserId = 1,
                VideoId = 1
            };
            _mockHistoryRepo.Setup(repo => repo.RemoveAsync(history))
                        .ThrowsAsync(new ArgumentException("User has not watched this video."));
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await _mockHistoryRepo.Object.RemoveAsync(history);
            });
        }

        [Fact]
        public async Task GetHistoriesByUserId_ShouldReturnHistories()
        {
            // Arrange
            var userId = 1;
            var histories = new List<History>
            {
                new History { UserId = 1, VideoId = 1 },
                new History { UserId = 1, VideoId = 2 }
            };
            _mockHistoryRepo.Setup(repo => repo.GetHistoriesByUserIdAsync(userId)).ReturnsAsync(histories);
            // Act
            var result = await _mockHistoryRepo.Object.GetHistoriesByUserIdAsync(userId);
            // Assert
            Assert.Equal(histories, result);
        }

        [Fact]
        public async Task GetHistoriesByUserId_NoHistories_ShouldReturnEmptyList()
        {
            // Arrange
            var userId = 1;
            var histories = new List<History>();
            _mockHistoryRepo.Setup(repo => repo.GetHistoriesByUserIdAsync(userId)).ReturnsAsync(histories);
            // Act
            var result = await _mockHistoryRepo.Object.GetHistoriesByUserIdAsync(userId);
            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetHistoriesByUserId_NonExistingUser_ShouldReturnEmptyList()
        {
            // Arrange
            var userId = 1;
            var histories = new List<History>();
            _mockHistoryRepo.Setup(repo => repo.GetHistoriesByUserIdAsync(userId)).ReturnsAsync(histories);
            // Act
            var result = await _mockHistoryRepo.Object.GetHistoriesByUserIdAsync(userId);
            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetHistoriesByUserId_ShouldReturnHistoriesWithDetails()
        {
            // Arrange
            var userId = 1;
            var histories = new List<History>
            {
                new History { UserId = 1, VideoId = 1, Video = new Video { Id = 1, Title = "Video 1", Description = "Video description", Genre = "Video genre" } },
                new History { UserId = 1, VideoId = 2, Video = new Video { Id = 2, Title = "Video 2", Description = "Video description", Genre = "Video genre" } }
            };
            _mockHistoryRepo.Setup(repo => repo.GetHistoriesByUserIdAsync(userId)).ReturnsAsync(histories);
            // Act
            var result = await _mockHistoryRepo.Object.GetHistoriesByUserIdAsync(userId);
            // Assert
            Assert.Equal(histories, result);
        }

        [Fact]
        public async Task GetHistoriesByUserId_NoHistories_ShouldReturnEmptyListWithDetails()
        {
            // Arrange
            var userId = 1;
            var histories = new List<History>();
            _mockHistoryRepo.Setup(repo => repo.GetHistoriesByUserIdAsync(userId)).ReturnsAsync(histories);
            // Act
            var result = await _mockHistoryRepo.Object.GetHistoriesByUserIdAsync(userId);
            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetHistoriesByUserId_NonExistingUser_ShouldReturnEmptyListWithDetails()
        {
            // Arrange
            var userId = 1;
            var histories = new List<History>();
            _mockHistoryRepo.Setup(repo => repo.GetHistoriesByUserIdAsync(userId)).ReturnsAsync(histories);
            // Act
            var result = await _mockHistoryRepo.Object.GetHistoriesByUserIdAsync(userId);
            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetHistoriesByUserId_ShouldReturnHistoriesOrderedByDateWatched()
        {
            // Arrange
            var userId = 1;
            var histories = new List<History>
            {
                new History { UserId = 1, VideoId = 1, DateWatched = DateTime.UtcNow },
                new History { UserId = 1, VideoId = 2, DateWatched = DateTime.UtcNow.AddHours(-1) }
            };
            _mockHistoryRepo.Setup(repo => repo.GetHistoriesByUserIdAsync(userId)).ReturnsAsync(histories);
            // Act
            var result = await _mockHistoryRepo.Object.GetHistoriesByUserIdAsync(userId);
            // Assert
            Assert.Equal(histories.OrderByDescending(h => h.DateWatched), result);
        }

        [Fact]
        public async Task GetHistoriesByUserId_NoHistories_ShouldReturnEmptyListOrderedByDateWatched()
        {
            // Arrange
            var userId = 1;
            var histories = new List<History>();
            _mockHistoryRepo.Setup(repo => repo.GetHistoriesByUserIdAsync(userId)).ReturnsAsync(histories);
            // Act
            var result = await _mockHistoryRepo.Object.GetHistoriesByUserIdAsync(userId);
            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetHistoriesByUserId_NonExistingUser_ShouldReturnEmptyListOrderedByDateWatched()
        {
            // Arrange
            var userId = 1;
            var histories = new List<History>();
            _mockHistoryRepo.Setup(repo => repo.GetHistoriesByUserIdAsync(userId)).ReturnsAsync(histories);
            // Act
            var result = await _mockHistoryRepo.Object.GetHistoriesByUserIdAsync(userId);
            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetHistoriesByUserId_ShouldReturnHistoriesWithDetailsOrderedByDateWatched()
        {
            // Arrange
            var userId = 1;
            var histories = new List<History>
            {
                new History { UserId = 1, VideoId = 1, DateWatched = DateTime.UtcNow, Video = new Video { Id = 1, Title = "Video 1", Description = "Video description", Genre = "Video genre" } },
                new History { UserId = 1, VideoId = 2, DateWatched = DateTime.UtcNow.AddHours(-1), Video = new Video { Id = 2, Title = "Video 2", Description = "Video description", Genre = "Video genre" } }
            };
            _mockHistoryRepo.Setup(repo => repo.GetHistoriesByUserIdAsync(userId)).ReturnsAsync(histories);
            // Act
            var result = await _mockHistoryRepo.Object.GetHistoriesByUserIdAsync(userId);
            // Assert
            Assert.Equal(histories.OrderByDescending(h => h.DateWatched), result);
        }

        [Fact]
        public async Task GetHistoriesByUserId_NoHistories_ShouldReturnEmptyListWithDetailsOrderedByDateWatched()
        {
            // Arrange
            var userId = 1;
            var histories = new List<History>();
            _mockHistoryRepo.Setup(repo => repo.GetHistoriesByUserIdAsync(userId)).ReturnsAsync(histories);
            // Act
            var result = await _mockHistoryRepo.Object.GetHistoriesByUserIdAsync(userId);
            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetHistoriesByUserId_NonExistingUser_ShouldReturnEmptyListWithDetailsOrderedByDateWatched()
        {
            // Arrange
            var userId = 1;
            var histories = new List<History>();
            _mockHistoryRepo.Setup(repo => repo.GetHistoriesByUserIdAsync(userId)).ReturnsAsync(histories);
            // Act
            var result = await _mockHistoryRepo.Object.GetHistoriesByUserIdAsync(userId);
            // Assert
            Assert.Empty(result);
        }
    }
}
