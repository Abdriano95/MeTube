using MeTube.Data.Repository;
using MeTube.Data;
using Microsoft.EntityFrameworkCore;
using Moq;
using MeTube.Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeTube.Test.Repositories
{
    public class UserRepositoryTests
    {
        private readonly Mock<IUserRepository> _mockUserRepo;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;

        public UserRepositoryTests()
        {
            _mockUserRepo = new Mock<IUserRepository>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockUnitOfWork.Setup(uow => uow.Users).Returns(_mockUserRepo.Object);
        }

        [Fact]
        public async Task AddUserAsync_ShouldAddUserSuccessfully()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                Username = "TestUser",
                Password = "Hej123",
                Email = "hej@gmail.com",
                Role = "User",
            };

            _mockUserRepo.Setup(repo => repo.AddUserAsync(user)).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(uow => uow.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            await _mockUserRepo.Object.AddUserAsync(user);
            await _mockUnitOfWork.Object.SaveChangesAsync();

            // Assert
            _mockUserRepo.Verify(repo => repo.AddUserAsync(user), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task AddUserAsync_DuplicateEmail_ShouldThrowException()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                Username = "TestUser",
                Password = "Hej123",
                Email = "hej@gmail.com",
                Role = "User",
            };

            _mockUserRepo.Setup(repo => repo.AddUserAsync(user))
                         .ThrowsAsync(new ArgumentException("Email already exists."));

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await _mockUserRepo.Object.AddUserAsync(user);
            });
        }

        [Fact]
        public async Task DeleteUserAsync_ShouldDeleteUserSuccessfully()
        {
            // Arrange
            var userId = 1;

            _mockUserRepo.Setup(repo => repo.DeleteUserAsync(userId)).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(uow => uow.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            await _mockUserRepo.Object.DeleteUserAsync(userId);
            await _mockUnitOfWork.Object.SaveChangesAsync();

            // Assert
            _mockUserRepo.Verify(repo => repo.DeleteUserAsync(userId), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteUserAsync_NonExistentUser_ShouldThrowException()
        {
            // Arrange
            var userId = 1;

            _mockUserRepo.Setup(repo => repo.DeleteUserAsync(userId))
                         .ThrowsAsync(new ArgumentException("User not found."));

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await _mockUserRepo.Object.DeleteUserAsync(userId);
            });
        }

        [Fact]
        public async Task GetUserByEmailAsync_ShouldReturnUserIfExists()
        {
            // Arrange
            var email = "test@example.com";
            var user = new User
            {
                Id = 1,
                Email = email,
                Username = "TestUser",
                Password = "securepassword",
                Role = "User"
            };

            _mockUserRepo.Setup(repo => repo.GetUserByEmailAsync(email)).ReturnsAsync(user);

            // Act
            var result = await _mockUserRepo.Object.GetUserByEmailAsync(email);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("TestUser", result.Username);
            _mockUserRepo.Verify(repo => repo.GetUserByEmailAsync(email), Times.Once);
        }

        [Fact]
        public async Task GetUserByEmailAsync_ShouldReturnNullIfUserDoesNotExist()
        {
            // Arrange
            var email = "nonexistent@example.com";

            _mockUserRepo.Setup(repo => repo.GetUserByEmailAsync(email)).ReturnsAsync((User)null);

            // Act
            var result = await _mockUserRepo.Object.GetUserByEmailAsync(email);

            // Assert
            Assert.Null(result);
            _mockUserRepo.Verify(repo => repo.GetUserByEmailAsync(email), Times.Once);
        }
    }
}
