using MeTube.Data.Repository;
using MeTube.Data;
using Microsoft.EntityFrameworkCore;
using MeTube.Data.Entity;

namespace MeTube.Test.Repositories
{
    public class VideoRepositoryTests
    {
        private readonly ApplicationDbContext _context;
        private readonly VideoRepository _repository;

        public VideoRepositoryTests()
        {
            // 1) Skapa in memory-databas med unikt namn (GUID) för varje testkörning
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{System.Guid.NewGuid()}")
                .Options;

            _context = new ApplicationDbContext(options);
            _repository = new VideoRepository(_context);

            // Seed data
            SeedData();
        }

        /// <summary>
        /// Exempel på hur man seedar Users, Videos, Likes m.m.
        /// </summary>
        private void SeedData()
        {
            // Skapa några users
            var user1 = new User { Id = 1, Username = "User1", Password = "pass", Email = "user1@test.com", Role = "User" };
            var user2 = new User { Id = 2, Username = "User2", Password = "pass", Email = "user2@test.com", Role = "User" };
            var user3 = new User { Id = 3, Username = "User3", Password = "pass", Email = "user3@test.com", Role = "User" };

            _context.Users.AddRange(user1, user2, user3);

            // Skapa några videos
            var videoA = new Video
            {
                Id = 10,
                UserId = 1,
                Title = "Video A",
                Description = "Desc A",
                Genre = "Rock",
                DateUploaded = System.DateTime.Now.AddDays(-5)
            };
            var videoB = new Video
            {
                Id = 11,
                UserId = 2,
                Title = "Video B",
                Description = "Desc B",
                Genre = "Rock",
                DateUploaded = System.DateTime.Now.AddDays(-2)
            };
            var videoC = new Video
            {
                Id = 12,
                UserId = 1,
                Title = "Video C",
                Description = "Desc C",
                Genre = "Pop",
                DateUploaded = System.DateTime.Now.AddDays(-3)
            };
            var videoD = new Video
            {
                Id = 13,
                UserId = 3,
                Title = "Video D",
                Description = "Desc D",
                Genre = "Pop",
                DateUploaded = System.DateTime.Now.AddDays(-1)
            };
            var videoE = new Video
            {
                Id = 14,
                UserId = 3,
                Title = "Video E",
                Description = "Desc E",
                Genre = "Rock",
                DateUploaded = System.DateTime.Now.AddDays(-10)
            };

            _context.Videos.AddRange(videoA, videoB, videoC, videoD, videoE);

            // Skapa några Likes
            var like1 = new Like { UserID = 1, VideoID = 11 };  // User1 gillar VideoB (Rock)
            var like2 = new Like { UserID = 1, VideoID = 12 };  // User1 gillar VideoC (Pop)
            var like3 = new Like { UserID = 2, VideoID = 10 };  // User2 gillar VideoA (Rock)
            var like4 = new Like { UserID = 2, VideoID = 12 };  // User2 gillar VideoC (Pop)
                                                                // user3 gillar inga videos

            _context.Likes.AddRange(like1, like2, like3, like4);

            _context.SaveChanges();
        }

        [Fact]
        public async Task GetRecommendedVideosForUserAsync_NoLikes_ReturnsRecentVideos()
        {
            // Arrange
            // User3 (id=3) har inga likes => fallback-läge

            // Act
            var result = await _repository.GetRecommendedVideosForUserAsync(userId: 3, maxCount: 3);

            // Assert
            Assert.NotNull(result);
            var list = result.ToList();
            // Ska returnera de senaste 3 uppladdade videorna
            // Seeding: Video D(d-1), B(d-2), C(d-3), A(d-5), E(d-10)
            // => De senaste 3: D(13), B(11), C(12)
            Assert.Equal(3, list.Count);
            Assert.Contains(list, v => v.Id == 13); // Video D
            Assert.Contains(list, v => v.Id == 11); // Video B
            Assert.Contains(list, v => v.Id == 12); // Video C
        }

        [Fact]
        public async Task GetRecommendedVideosForUserAsync_HasLikes_ReturnsTopGenre()
        {
            // Arrange
            // User1 gillar video 11 (Rock) och 12 (Pop).
            // LikedGenres => { "Rock", "Pop" }
            // topGenre => "Rock" or "Pop", 
            //    men i denna implementation tar vi den mest förekommande 
            //    (båda är 1-1 => "Rock" hamnar sist i groupby?? => 
            // i koden: groupby => orderby descending => first => "Rock" om ordningen är A->Z. 
            // Egentligen i en draw. 
            // Låt oss anta att "Rock" kommer först.

            // Act
            var result = await _repository.GetRecommendedVideosForUserAsync(userId: 1, maxCount: 10);

            // Assert
            var list = result.ToList();

            // topGenre => "Rock" => Return videos i "Rock", exkl user1's already liked 
            // user1 har gillat (11, "Rock") => exclude that
            // user1 äger video 10, 12 => men 10 är "Rock", 12 är "Pop"
            // Egentligen i metoden exkluderas "liked" videos => 11 
            //  => Kvar i "Rock": 10 (User1:s egen?), 14 (User3s)...

            // Koden exkluderar "likedVideoIds" => 11
            // Koden säger *vi exkluderar inte user1's own videos i exempler? 
            //     "and !likedVideoIds.Contains(v.Id)" men inte user1's own 
            // Du skrev i kod: "Exclude the user's own videos"? 
            // I exemplet => .Where(v => v.Genre == topGenre && !likedVideoIds.Contains(v.Id))
            // men ej exkl. v.UserId == userId ?

            // Just nu exkluderas enbart 11, 
            // => Kvar i "Rock": 10 (Video A, userId=1), 14 (Video E, userId=3). 
            // Metoden tar de senast uppladdade => (10 är d-5), (14 är d-10), 
            // => ordna desc => (10, 14). 
            // => top(10)...

            Assert.NotEmpty(list);
            // Beroende på seeds => bör få video10, video14
            //  (om "Rock" valts)
            Assert.Contains(list, v => v.Id == 10);
            Assert.Contains(list, v => v.Id == 14);
            // We also ensure that 11 is excluded
            Assert.DoesNotContain(list, v => v.Id == 11);

            // => If the topGenre was "Pop", you'd check for videoC(12) or D(13)
            //    But user1 already liked 12 => excludes that
            //    => maybe 13 => in pop
        }

        [Fact]
        public async Task GetRecommendedVideosForUserAsync_MaxCountIsRespected()
        {
            // Arrange
            // user2 gillar 10(Rock), 12(Pop) => topGenre?
            // same approach => "Rock" or "Pop". Suppose "Rock" if tie
            // "Rock" has 1 like, "Pop" has 1 => same. => probably "Rock"
            // videos in "Rock": 10, 11, 14 => user2 liked 10, exclude => => 11,14
            // order => 11(d-2),14(d-10) => take(1)

            // Act
            var result = await _repository.GetRecommendedVideosForUserAsync(userId: 2, maxCount: 1);

            // Assert
            Assert.Single(result);
            var onlyVid = result.FirstOrDefault();
            // Probably video 11, which is "Rock" & not liked by user2? 
            // Actually user2 has liked 10(Rock) => exclude => leftover 11,14
            //  => sorted desc => 11(d-2),14(d-10)
            //  => top(1) => => video 11
            Assert.NotNull(onlyVid);
            Assert.Equal(11, onlyVid.Id);
        }

        [Fact]
        public async Task GetRecommendedVideosForUserAsync_UserHasOnlyOneLikedGenre_ReturnsOnlyThatGenre()
        {
            // Arrange
            // User1 gillar bara "Rock" => bör få "Rock"-videor

            var userId = 1;
            await _context.Likes.AddAsync(new Like { UserID = userId, VideoID = 10 }); // Rock
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetRecommendedVideosForUserAsync(userId, maxCount: 5);

            // Assert
            Assert.NotEmpty(result);
            Assert.All(result, v => Assert.Equal("Rock", v.Genre)); // Alla rekommenderade videor bör vara "Rock"
        }

        [Fact]
        public async Task GetRecommendedVideosForUserAsync_UserHasEqualGenres_PicksRandomOrFirst()
        {
            // Arrange
            // User3 gillar lika många Rock & Pop => Resultatet kan vara båda
            var userId = 3;
            await _context.Likes.AddRangeAsync(new List<Like>
            {
                new Like { UserID = userId, VideoID = 10 }, // Rock
                new Like { UserID = userId, VideoID = 12 }, // Pop
            });
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetRecommendedVideosForUserAsync(userId, maxCount: 5);

            // Assert
            Assert.NotEmpty(result);
            var genres = result.Select(v => v.Genre).Distinct().ToList();
            Assert.True(genres.Contains("Rock") || genres.Contains("Pop"));
        }

        [Fact]
        public async Task GetRecommendedVideosForUserAsync_UserHasLikedAllVideosInTopGenre_ReturnsEmpty()
        {
            // Arrange
            var userId = 2;

            // Säkerställ att vi inte spårar dubbletter
            var existingLikes = _context.Likes.Where(l => l.UserID == userId).ToList();
            _context.Likes.RemoveRange(existingLikes);
            await _context.SaveChangesAsync();

            // Lägg till likes (användaren har redan gillat alla "Rock"-videor)
            var likes = new List<Like>
            {
                new Like { UserID = userId, VideoID = 10 }, // Rock
                new Like { UserID = userId, VideoID = 11 }, // Rock
                new Like { UserID = userId, VideoID = 14 }  // Rock
            };

            _context.Likes.AddRange(likes);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetRecommendedVideosForUserAsync(userId, maxCount: 5);

            // Assert
            Assert.Empty(result); // Alla Rock-videor är redan gillade
        }

        [Fact]
        public async Task GetRecommendedVideosForUserAsync_UserHasMultipleGenres_ChoosesMostLikedGenre()
        {
            // Arrange
            var userId = 2;

            // Ta bort existerande likes innan vi lägger till nya
            var existingLikes = _context.Likes.Where(l => l.UserID == userId).ToList();
            _context.Likes.RemoveRange(existingLikes);
            await _context.SaveChangesAsync();

            // Lägg till likes
            await _context.Likes.AddRangeAsync(new List<Like>
            {
                new Like { UserID = userId, VideoID = 10 }, // Rock
                new Like { UserID = userId, VideoID = 11 }, // Rock
                new Like { UserID = userId, VideoID = 12 }  // Pop
            });
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetRecommendedVideosForUserAsync(userId, maxCount: 5);

            // Assert
            Assert.NotEmpty(result);
            Assert.All(result, v => Assert.Equal("Rock", v.Genre)); // Rock ska vara dominant
        }

        [Fact]
        public async Task GetRecommendedVideosForUserAsync_UserHasNoMoreVideosInTopGenre_ReturnsEmpty()
        {
            // Arrange
            var userId = 1;

            // Ta bort alla videor i top-genren och eventuella andra genrer för att undvika fallback
            var videosToRemove = _context.Videos
                .Where(v => v.Genre == "Rock" || v.Genre == "Pop") // Ta bort även Pop om metoden väljer en annan genre
                .ToList();

            _context.Videos.RemoveRange(videosToRemove);
            await _context.SaveChangesAsync();

            // Lägg till en like (användaren gillar Rock, men genren ska vara borta)
            await _context.Likes.AddAsync(new Like { UserID = userId, VideoID = 10 });
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetRecommendedVideosForUserAsync(userId, maxCount: 5);

            // Assert
            Assert.Empty(result); // Det ska nu inte finnas några videor att rekommendera
        }


        [Fact]
        public async Task GetRecommendedVideosForUserAsync_UserHasNoVideosInDatabase_ReturnsEmpty()
        {
            // Arrange
            var userId = 99;

            // Radera alla videor manuellt istället för ExecuteDeleteAsync()
            var allVideos = _context.Videos.ToList();
            _context.Videos.RemoveRange(allVideos);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetRecommendedVideosForUserAsync(userId, maxCount: 5);

            // Assert
            Assert.Empty(result); // Finns inga videor att rekommendera
        }

    }
}
    