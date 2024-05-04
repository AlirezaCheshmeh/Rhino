using Microsoft.EntityFrameworkCore;

namespace Application.Database
{
    public interface IApplicationDataContext : IDisposable, IAsyncDisposable
    {
        int SaveChanges();
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        DbSet<TEntity> Set<TEntity>() where TEntity : class;
    }
}
