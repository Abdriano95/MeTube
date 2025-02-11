using AutoMapper;
using MeTube.API.Controllers;
using MeTube.API.Services;
using MeTube.Data.Entity;
using MeTube.Data.Repository;
using MeTube.DTO.VideoDTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using Xunit;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace MeTube.Test.APIControllers
{
    public class VideoControllerTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<VideoService> _mockVideoService;
        private readonly VideoController _controller;

        private readonly List<Video> _videos;

        public VideoControllerTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();

           
            // mockar
            _mockVideoService = new Mock<VideoService>();

            // Skapa en standardanvändare (UserId = 1) via ClaimsPrincipal
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "1"), // "User" med ID=1
                new Claim(ClaimTypes.Role, "User")         // Rolle: "User"
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            // Instansav controllern
            _controller = new VideoController(_mockUnitOfWork.Object, _mockMapper.Object, _mockVideoService.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = claimsPrincipal
                    }
                }
            };

            // Exempellista med några videor
            _videos = new List<Video>
            {
                new Video { Id = 1, Title = "Video #1", Description="Desc #1", Genre="Genre #1", UserId=1, BlobName="blob1.mp4", VideoUrl="http://blob1.mp4", ThumbnailUrl="http://thumb1.jpg" },
                new Video { Id = 2, Title = "Video #2", Description="Desc #2", Genre="Genre #2", UserId=2, BlobName="blob2.mp4", VideoUrl="http://blob2.mp4", ThumbnailUrl="http://thumb2.jpg" }
            };

        }
    }
}

        