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
        private readonly ApplicationDbContext _dbContext;
        private readonly VideoRepository _videoRepository;

        public VideoRepositoryTest()
        {
           var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "MeTubeTestDB")
                .Options;
            _dbContext = new ApplicationDbContext(options);
            
        }
    }
}
