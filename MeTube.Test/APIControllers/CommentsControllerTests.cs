using AutoMapper;
using MeTube.API.Controllers;
using MeTube.Data.Entity;
using MeTube.Data.Repository;
using MeTube.DTO;
using MeTube.DTO.CommentDTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace MeTube.Test.APIControllers
{
    public class CommentsControllerTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IMapper> _mockMapper;
        private readonly CommentsController _controller;
        private readonly List<Comment> _comments;

        public CommentsControllerTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _controller = new CommentsController(_mockUnitOfWork.Object, _mockMapper.Object);
            _comments = new List<Comment>
            {
                new Comment { Id = 1, VideoId = 1, UserId = 1, Content = "Test Comment 1", DateAdded = DateTime.UtcNow },
                new Comment { Id = 2, VideoId = 1, UserId = 2, Content = "Test Comment 2", DateAdded = DateTime.UtcNow }
            };

            // Setup ClaimsPrincipal for authenticated requests
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Role, "Admin")
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = claimsPrincipal
                }
            };
        }

        [Fact]
        public async Task GetCommentsByVideo_ReturnsOkWithData()
        {
            // Arrange
            _mockUnitOfWork.Setup(uow => uow.Comments.GetCommentsByVideoIdAsync(1)).ReturnsAsync(_comments);
            var commentDtos = _comments.Select(c => new CommentDto { Id = c.Id, VideoId = c.VideoId, UserId = c.UserId, Content = c.Content, DateAdded = c.DateAdded });
            _mockMapper.Setup(m => m.Map<IEnumerable<CommentDto>>(_comments)).Returns(commentDtos);

            // Act
            var result = await _controller.GetCommentsByVideo(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<CommentDto>>(okResult.Value);
            Assert.Equal(2, model.Count());
        }

        [Fact]
        public async Task GetCommentsByUser_ReturnsNotFound_WhenNoCommentsExist()
        {
            // Arrange
            _mockUnitOfWork.Setup(uow => uow.Comments.GetCommentsByUserIdAsync(1)).ReturnsAsync(new List<Comment>());

            // Act
            var result = await _controller.GetCommentsByUser(1);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task GetCommentsByUser_ReturnsOkWithData()
        {
            // Arrange
            _mockUnitOfWork.Setup(uow => uow.Comments.GetCommentsByUserIdAsync(1)).ReturnsAsync(_comments);
            var commentDtos = _comments.Select(c => new CommentDto { Id = c.Id, VideoId = c.VideoId, UserId = c.UserId, Content = c.Content, DateAdded = c.DateAdded });
            _mockMapper.Setup(m => m.Map<IEnumerable<CommentDto>>(_comments)).Returns(commentDtos);

            // Act
            var result = await _controller.GetCommentsByUser(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<CommentDto>>(okResult.Value);
            Assert.Equal(2, model.Count());
        }

        [Fact]
        public async Task AddComment_ReturnsOk_WhenCommentIsAdded()
        {
            // Arrange
            var commentDto = new CommentDto { Id = 1, VideoId = 1, UserId = 1, Content = "Test Comment", DateAdded = DateTime.UtcNow };
            var comment = new Comment { Id = 1, VideoId = 1, UserId = 1, Content = "Test Comment", DateAdded = DateTime.UtcNow };
            var video = new Video { Id = 1, Title = "Test Video", Description = "Test Description", Genre = "Test Genre", DateUploaded = DateTime.UtcNow };

            _mockMapper.Setup(m => m.Map<Comment>(commentDto)).Returns(comment);
            _mockUnitOfWork.Setup(u => u.Comments.AddCommentAsync(It.IsAny<Comment>())).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
            _mockUnitOfWork.Setup(u => u.Videos.GetByIdAsync(commentDto.VideoId)).ReturnsAsync(video);

            // Act
            var result = await _controller.AddComment(commentDto);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task AddComment_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
            _controller.ModelState.AddModelError("Content", "Required");

            var commentDto = new CommentDto { Id = 1, VideoId = 1, UserId = 1, Content = "", DateAdded = DateTime.UtcNow };

            // Act
            var result = await _controller.AddComment(commentDto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task DeleteComment_ReturnsOk_WhenCommentIsDeleted()
        {
            // Arrange
            var comment = new Comment { Id = 1, VideoId = 1, UserId = 1, Content = "Test Comment", DateAdded = DateTime.UtcNow };

            _mockUnitOfWork.Setup(u => u.Comments.GetCommentByIdAsync(1)).ReturnsAsync(comment);
            _mockUnitOfWork.Setup(u => u.Comments.DeleteComment(comment));
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            var result = await _controller.DeleteComment(1);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task DeleteComment_ReturnsNotFound_WhenCommentDoesNotExist()
        {
            // Arrange
            _mockUnitOfWork.Setup(u => u.Comments.GetCommentByIdAsync(1)).ReturnsAsync((Comment)null);

            // Act
            var result = await _controller.DeleteComment(1);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task UpdateComment_ReturnsOk_WhenCommentIsUpdated()
        {
            // Arrange
            var updateCommentDto = new UpdateCommentDto { Content = "Updated Comment" };
            var existingComment = new Comment { Id = 1, VideoId = 1, UserId = 1, Content = "Test Comment", DateAdded = DateTime.UtcNow };

            _mockUnitOfWork.Setup(u => u.Comments.GetCommentByIdAsync(1)).ReturnsAsync(existingComment);
            _mockMapper.Setup(m => m.Map(updateCommentDto, existingComment));
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            var result = await _controller.UpdateComment(1, updateCommentDto);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task UpdateComment_ReturnsNotFound_WhenCommentDoesNotExist()
        {
            // Arrange
            var updateCommentDto = new UpdateCommentDto { Content = "Updated Comment" };

            _mockUnitOfWork.Setup(u => u.Comments.GetCommentByIdAsync(1)).ReturnsAsync((Comment)null);

            // Act
            var result = await _controller.UpdateComment(1, updateCommentDto);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task UpdateComment_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
            _controller.ModelState.AddModelError("Content", "Required");

            var updateCommentDto = new UpdateCommentDto { Content = "" };

            // Act
            var result = await _controller.UpdateComment(1, updateCommentDto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}
