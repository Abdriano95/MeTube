using MeTube.Data;
using MeTube.Data.Entity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MeTube.Test.Repositories
{
    public class UserSeedDataTests
    {
        [Theory]
        [InlineData("admin", "adminpwd123")]
        [InlineData("john_doe", "password123")]
        [InlineData("jane_smith", "password456")]
        [InlineData("tech_guru", "techpass789")]
        [InlineData("sports_fan", "sportspass789")]
        public void SeededUser_PasswordHash_ShouldVerifyAgainstDemoPassword(string username, string demoPassword)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var user = context.Users.Single(u => u.Username == username);
            var result = new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash, demoPassword);

            Assert.NotEqual(demoPassword, user.PasswordHash);
            Assert.Equal(PasswordVerificationResult.Success, result);
        }
    }
}
