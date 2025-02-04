using MeTube.Data.Entity;
using MeTube.Data.Repository;
using Moq;

namespace MeTube.Test.Repositories
{
    public class LikeRepositoryTests
    {
        private readonly Mock<ILikeRepository> _mockLikeRepo;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;

        public LikeRepositoryTests()
        {
            _mockLikeRepo = new Mock<ILikeRepository>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockUnitOfWork.Setup(uow => uow.Likes).Returns(_mockLikeRepo.Object);
        }

        [Fact]
        public async Task AddLike_ShouldAddSuccessfully()
        {
            // Arrange
            var like = new Like
            {
                UserID = 1,
                VideoID = 1
            };
            _mockLikeRepo.Setup(repo => repo.AddLikeAsync(like)).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(uow => uow.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            await _mockLikeRepo.Object.AddLikeAsync(like);
            await _mockUnitOfWork.Object.SaveChangesAsync();

            // Assert
            _mockLikeRepo.Verify(repo => repo.AddLikeAsync(like), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task AddLike_DuplicateLike_ShouldThrowException()
        {
            // Arrange
            var like = new Like
            {
                UserID = 1,
                VideoID = 1
            };
            _mockLikeRepo.Setup(repo => repo.AddLikeAsync(like))
                        .ThrowsAsync(new ArgumentException("User has already liked this video."));

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await _mockLikeRepo.Object.AddLikeAsync(like);
            });
        }

        [Fact]
        public async Task RemoveLike_ShouldRemoveSuccessfully()
        {
            // Arrange
            var like = new Like
            {
                UserID = 1,
                VideoID = 1
            };
            _mockLikeRepo.Setup(repo => repo.RemoveLikeAsync(like)).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(uow => uow.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            await _mockLikeRepo.Object.RemoveLikeAsync(like);
            await _mockUnitOfWork.Object.SaveChangesAsync();

            // Assert
            _mockLikeRepo.Verify(repo => repo.RemoveLikeAsync(like), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task RemoveLike_NonexistentLike_ShouldThrowException()
        {
            // Arrange
            var like = new Like
            {
                UserID = 1,
                VideoID = 1
            };
            _mockLikeRepo.Setup(repo => repo.RemoveLikeAsync(like))
                        .ThrowsAsync(new ArgumentException("Like does not exist."));

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await _mockLikeRepo.Object.RemoveLikeAsync(like);
            });
        }

        [Fact]
        public async Task GetAllLikes_ShouldReturnAllLikes()
        {
            // Arrange
            var likes = new List<Like>
            {
                new Like { UserID = 1, VideoID = 1 },
                new Like { UserID = 2, VideoID = 1 }
            };
            _mockLikeRepo.Setup(repo => repo.GetAllAsync()).ReturnsAsync(likes);

            // Act
            var result = await _mockLikeRepo.Object.GetAllAsync();

            // Assert
            Assert.Equal(2, result.Count());
            _mockLikeRepo.Verify(repo => repo.GetAllAsync(), Times.Once);
        }
    }
}
