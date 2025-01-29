namespace MeTube.Data.Repository
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository Users { get; }
        IVideoRepository Videos { get; }

        Task<int> SaveChangesAsync();
    }
}
