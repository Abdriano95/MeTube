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
    public class CommentRepositoryTests
    {
        private readonly Mock<ICommentRepository> _mockCommentRepo;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;

        public CommentRepositoryTests()
        {
            _mockCommentRepo = new Mock<ICommentRepository>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();

            _mockUnitOfWork
                .Setup(uow => uow.SaveChangesAsync())
                .ReturnsAsync(1);

            _mockUnitOfWork
                .Setup(uow => uow.Comments)
                .Returns(_mockCommentRepo.Object);
        }

        [Fact]
        public async Task GetCommentsByVideoIdAsync_ShouldReturnListOfComments()
        {
            // Arrange
            var videoId = 1;
            var comments = new List<Comment>
            {
                new Comment { Id = 1, VideoId = videoId, UserId = 1, Content = "First comment", DateAdded = DateTime.UtcNow },
                new Comment { Id = 2, VideoId = videoId, UserId = 2, Content = "Second comment", DateAdded = DateTime.UtcNow }
            };

            _mockCommentRepo
                .Setup(repo => repo.GetCommentsByVideoIdAsync(videoId))
                .ReturnsAsync(comments);

            // Act
            var result = await _mockCommentRepo.Object.GetCommentsByVideoIdAsync(videoId);

            // Assert
            result.Should().NotBeNull().And.HaveCount(2);
            result.First().VideoId.Should().Be(videoId);
            _mockCommentRepo.Verify(repo => repo.GetCommentsByVideoIdAsync(videoId), Times.Once);
        }

        [Fact]
        public async Task GetCommentsByUserIdAsync_ShouldReturnUserComments()
        {
            // Arrange
            var userId = 1;
            var comments = new List<Comment>
            {
                new Comment { Id = 1, VideoId = 1, UserId = userId, Content = "User comment 1", DateAdded = DateTime.UtcNow },
                new Comment { Id = 2, VideoId = 2, UserId = userId, Content = "User comment 2", DateAdded = DateTime.UtcNow }
            };

            _mockCommentRepo
                .Setup(repo => repo.GetCommentsByUserIdAsync(userId))
                .ReturnsAsync(comments);

            // Act
            var result = await _mockCommentRepo.Object.GetCommentsByUserIdAsync(userId);

            // Assert
            result.Should().NotBeNull().And.HaveCount(2);
            result.First().UserId.Should().Be(userId);
            _mockCommentRepo.Verify(repo => repo.GetCommentsByUserIdAsync(userId), Times.Once);
        }

        [Fact]
        public async Task GetCommentByIdAsync_ExistingComment_ShouldReturnComment()
        {
            // Arrange
            var comment = new Comment { Id = 1, VideoId = 1, UserId = 1, Content = "Test comment", DateAdded = DateTime.UtcNow };

            _mockCommentRepo
                .Setup(repo => repo.GetCommentByIdAsync(1))
                .ReturnsAsync(comment);

            // Act
            var result = await _mockCommentRepo.Object.GetCommentByIdAsync(1);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(1);
            result.Content.Should().Be("Test comment");
            _mockCommentRepo.Verify(repo => repo.GetCommentByIdAsync(1), Times.Once);
        }

        [Fact]
        public async Task GetCommentByIdAsync_NonExistingComment_ShouldReturnNull()
        {
            // Arrange
            _mockCommentRepo
                .Setup(repo => repo.GetCommentByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((Comment?)null);

            // Act
            var result = await _mockCommentRepo.Object.GetCommentByIdAsync(999);

            // Assert
            result.Should().BeNull();
            _mockCommentRepo.Verify(repo => repo.GetCommentByIdAsync(999), Times.Once);
        }

        [Fact]
        public async Task AddCommentAsync_ValidComment_ShouldAddAndSave()
        {
            // Arrange
            var comment = new Comment { Id = 1, VideoId = 1, UserId = 1, Content = "New comment", DateAdded = DateTime.UtcNow };

            _mockCommentRepo
                .Setup(repo => repo.AddCommentAsync(comment))
                .Returns(Task.CompletedTask);

            // Act
            await _mockCommentRepo.Object.AddCommentAsync(comment);
            var saveResult = await _mockUnitOfWork.Object.SaveChangesAsync();

            // Assert
            saveResult.Should().Be(1);
            _mockCommentRepo.Verify(repo => repo.AddCommentAsync(comment), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task AddCommentAsync_NullComment_ShouldThrowException()
        {
            // Arrange
            _mockCommentRepo
                .Setup(repo => repo.AddCommentAsync(null!))
                .ThrowsAsync(new ArgumentNullException());

            // Act
            Func<Task> act = async () => await _mockCommentRepo.Object.AddCommentAsync(null!);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>();
            _mockCommentRepo.Verify(repo => repo.AddCommentAsync(null!), Times.Once);
        }

        [Fact]
        public void UpdateComment_ValidComment_ShouldUpdateAndSave()
        {
            // Arrange
            var comment = new Comment { Id = 1, VideoId = 1, UserId = 1, Content = "Updated comment", DateAdded = DateTime.UtcNow };

            _mockCommentRepo.Setup(repo => repo.UpdateComment(comment));

            // Act
            _mockCommentRepo.Object.UpdateComment(comment);
            var saveResult = _mockUnitOfWork.Object.SaveChangesAsync().Result;

            // Assert
            saveResult.Should().Be(1);
            _mockCommentRepo.Verify(repo => repo.UpdateComment(comment), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public void UpdateComment_NullComment_ShouldThrowException()
        {
            // Arrange
            _mockCommentRepo
                .Setup(repo => repo.UpdateComment(null!))
                .Throws<ArgumentNullException>();

            // Act
            Action act = () => _mockCommentRepo.Object.UpdateComment(null!);

            // Assert
            act.Should().Throw<ArgumentNullException>();
            _mockCommentRepo.Verify(repo => repo.UpdateComment(null!), Times.Once);
        }

        [Fact]
        public async Task DeleteComment_ValidComment_ShouldDeleteAndSave()
        {
            // Arrange
            var comment = new Comment { Id = 1, VideoId = 1, UserId = 1, Content = "To be deleted", DateAdded = DateTime.UtcNow };

            _mockCommentRepo
                .Setup(repo => repo.DeleteComment(comment));

            // Act
            _mockCommentRepo.Object.DeleteComment(comment);
            var saveResult = await _mockUnitOfWork.Object.SaveChangesAsync();

            // Assert
            saveResult.Should().Be(1);
            _mockCommentRepo.Verify(repo => repo.DeleteComment(comment), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task GetPosterUsernameAsync_ValidId_ShouldReturnUsername()
        {
            // Arrange
            var userId = 1;
            _mockCommentRepo
                .Setup(repo => repo.GetPosterUsernameAsync(userId))
                .ReturnsAsync("TestUser");

            // Act
            var username = await _mockCommentRepo.Object.GetPosterUsernameAsync(userId);

            // Assert
            username.Should().Be("TestUser");
            _mockCommentRepo.Verify(repo => repo.GetPosterUsernameAsync(userId), Times.Once);
        }
    }
}
