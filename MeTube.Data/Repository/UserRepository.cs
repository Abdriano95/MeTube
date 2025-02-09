using MeTube.Data.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeTube.Data.Repository
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public ApplicationDbContext DbContext => Context as ApplicationDbContext;

        public UserRepository(ApplicationDbContext context) : base(context)
        {
        }
        public async Task AddUserAsync(User user)
        {
            await AddAsync(user);
        }

        // if user has videos: Delete comments, likes, history for each video, then delete each video
        // else just delete the user
        // Ta först bort kommentarer, likes, history för varje video som användaren har
        // Sedan ta bort varje video
        // Sedan ta bort användaren
        public async Task DeleteUser(User user)
        {
            using var transaction = await DbContext.Database.BeginTransactionAsync();
            try
            {
                List<int> userVideos = DbContext.Videos.Where(a => a.UserId == user.Id).Select(b => b.Id).ToList();
                if (userVideos.Any())
                {
                    DbContext.Comments.Where(c => userVideos.Contains(c.VideoId)).ExecuteDelete();
                    DbContext.Likes.Where(l => userVideos.Contains(l.VideoID)).ExecuteDelete();
                    DbContext.Histories.Where(h => userVideos.Contains(h.VideoId)).ExecuteDelete();
                    DbContext.Videos.Where(v => userVideos.Contains(v.Id)).ExecuteDelete();
                }
                DbContext.Comments.Where(c => c.UserId == user.Id).ExecuteDelete();
                DbContext.Histories.Where(c => c.UserId == user.Id).ExecuteDelete();
                DbContext.Likes.Where(c => c.UserID == user.Id).ExecuteDelete();
                Delete(user);
                await transaction.CommitAsync();
            }
            catch (Exception ex) 
            {
                await transaction.RollbackAsync();
                throw new Exception("Kunde inte radera användaren.", ex);
            }
        }

        public async Task DeleteUserAsync(int userId)
        {
            using var transaction = await DbContext.Database.BeginTransactionAsync();
            try
            {
                // 1. Hämta användaren med ALLA relaterade entiteter
                var user = await DbContext.Users.Include(u => u.Videos).ThenInclude(v => v.Comments).Include(u => u.Videos).ThenInclude(v => v.Likes).Include(u => u.Videos).ThenInclude(v => v.Histories)
                    .Include(u => u.Comments)
                    .Include(u => u.Likes)
                    .Include(u => u.Histories)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null) return;

                // 2. Radera alla Video-relaterade entiteter
                foreach (var video in user.Videos)
                {
                    DbContext.Comments.RemoveRange(video.Comments); // Video's comments
                    DbContext.Likes.RemoveRange(video.Likes);       // Video's likes
                    DbContext.Histories.RemoveRange(video.Histories);
                    DbContext.Videos.Remove(video); // Radera videon själv
                }

                // 3. Radera användarens direkta entiteter
                DbContext.Comments.RemoveRange(user.Comments); // Kommentarer användaren skrivit på ANDRAS videos
                DbContext.Likes.RemoveRange(user.Likes);       // Likes användaren satt på ANDRAS videos
                DbContext.Histories.RemoveRange(user.Histories);

                // 4. Radera användaren
                DbContext.Users.Remove(user);

                // 5. Spara och commit
                await DbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception("Kunde inte radera användaren.", ex);
            }
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await DbContext.Users.AnyAsync(u => u.Email == email);
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await GetAllAsync();
        }

        public async Task<int?> GetUserIdByEmailAsync(string email)
        {
            var user = await DbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
            return user?.Id;
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await DbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await GetByIdAsync(id);
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await DbContext.Users.FirstOrDefaultAsync(u => u.Username == username);
        }

        public void UpdateUser(User user)
        {
            DbContext.Users.Update(user);
        }

        public async Task<bool> UsernameExistsAsync(string username)
        {
            return await DbContext.Users.AnyAsync(u => u.Username == username);
        }
    }
}
