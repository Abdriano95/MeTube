using Microsoft.EntityFrameworkCore.Storage;

namespace MeTube.Data.Repository
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository Users { get; }
        IVideoRepository Videos { get; }
        ILikeRepository Likes { get; }
        IHistoryRepository Histories { get; }
        ICommentRepository Comments { get; }

        Task<int> SaveChangesAsync();

        Task<IDbContextTransaction> BeginTransactionAsync();
        Task CommitAsync();
        Task RollbackAsync();

    }
}
