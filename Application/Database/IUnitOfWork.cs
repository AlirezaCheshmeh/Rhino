using Application.Common;
using Application.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Database
{
    public interface IUnitOfWork
    {
        IUserRepository UserRepository { get; }
        int Commit();
        Task<int> CommitAsync(CancellationToken cancellationToken = default);
        IGenericRepository<TEntity> GetRepository<TEntity>() where TEntity : class;
    }
}
