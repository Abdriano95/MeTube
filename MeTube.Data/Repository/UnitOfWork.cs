namespace MeTube.Data.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext context;

        public IUserRepository Users { get; private set; }

        public UnitOfWork(ApplicationDbContext context)
        {
            this.context = context;
            Users = new UserRepository(context);
        }
        public void Dispose()
        {
            context.Dispose();
        }

        public async Task<int> SaveChangesAsync()
        {
            return await context.SaveChangesAsync();
        }
    }
}
