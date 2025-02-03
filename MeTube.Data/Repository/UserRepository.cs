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
        public void DeleteUser(User user)
        {
            Delete(user);
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

        //public async Task<bool> ChangeUserRoleAsync(int userId, string newRole)
        //{
        //    var user = await DbContext.Users.FindAsync(userId);
        //    if (user == null)
        //        return false;

        //    // Kontrollera om användaren redan är Admin
        //    if (user is Admin)
        //        return false;

        //    // Skapa en ny Admin-instans baserat på användaren
        //    Admin newAdmin = new Admin
        //    {
        //        Id = userId,
        //        Username = user.Username,
        //        Email = user.Email,
        //        Password = user.Password
        //    };

        //    // Ta bort den gamla användaren och lägg till den nya Admin
        //    DbContext.Users.Remove(user);
        //    await DbContext.Users.AddAsync(newAdmin);

        //    await DbContext.SaveChangesAsync();
        //    return true;
        //}
    }
}
