using MeTube.Data.Entity;
using MeTube.Data.Repository;
using Moq;

namespace MeTube.Test
{
    public class VieoTest
    {
        // Enhetstest för att kontrollera att ID inte genereras vid AddVideoWithoutSave
        [Fact]
        public async Task AddVideoWithoutSave_ShouldNotGenerateId()
        {
            // Arrange
            var video = new Video
            {
                Title = "Test",
                Description = "Test Description",
                Genre = "Test Genre"
            };
            var mockRepo = new Mock<IVideoRepository>();
            mockRepo.Setup(repo => repo.AddVideoWithoutSaveAsync(video)).Returns(Task.CompletedTask);

            // Act
            await mockRepo.Object.AddVideoWithoutSaveAsync(video);

            // Assert
            Assert.Equal(0, video.Id); // ID ska genereras vid SaveChanges
        }

    }
}
