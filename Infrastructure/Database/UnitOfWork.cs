using Application.Common;
using Application.Database;
using Application.Extensions;
using Application.Repositories;
using Domain.Entities;
using Infrastructure.Repositories;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Database
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly Application.Database.IApplicationDataContext _context;
        private Hashtable _repositories;
        private UserRepository _userRepository;

        public UnitOfWork(Application.Database.IApplicationDataContext context)
        {
            _context = context;
            _repositories = new Hashtable();
        }


        public IUserRepository UserRepository => _userRepository ??= new(_context);

        public int Commit() => _context.SaveChanges();


        public async Task<int> CommitAsync(CancellationToken cancellationToken = default) => await _context.SaveChangesAsync(cancellationToken);


        public IGenericRepository<TEntity> GetRepository<TEntity>() where TEntity : class
        {
            if (_repositories == null) _repositories = new Hashtable();
            var Type = typeof(TEntity).Name;
            if (!_repositories.ContainsKey(Type))
            {
                var repositiryType = typeof(GenericRepository<>);
                var repositoryInstance = Activator.CreateInstance(
                    repositiryType.MakeGenericType(typeof(TEntity)), _context);
                _repositories.Add(Type, repositoryInstance);
            }
            return (IGenericRepository<TEntity>)_repositories[Type];
        }
    }
}
