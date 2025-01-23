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
