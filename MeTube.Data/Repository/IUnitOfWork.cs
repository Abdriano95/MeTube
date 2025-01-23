namespace MeTube.Data.Repository
{
    public interface IUnitOfWork : IDisposable
    {

        Task<int> SaveChangesAsync();
    }
}
