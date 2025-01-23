namespace MeTube.Data.Repository
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository Users { get; }  

        Task<int> SaveChangesAsync();
    }
}
