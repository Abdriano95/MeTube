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

    }  
}
