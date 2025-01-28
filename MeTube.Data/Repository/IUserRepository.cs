using MeTube.Data.Entity;
namespace MeTube.Data.Repository
{
    public interface IUserRepository : IRepository<User>
    {
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<User?> GetUserByIdAsync(int id);
        Task<User?> GetUserByEmailAsync(string email);
        Task<bool> EmailExistsAsync(string email);
        Task<bool> UsernameExistsAsync(string username);
        Task<User?> GetUserByUsernameAsync(string username);
        Task<int?> GetUserIdByEmailAsync(string email);
        Task AddUserAsync(User user);
        void UpdateUser(User user);
        void DeleteUser(User user);

    }
}
