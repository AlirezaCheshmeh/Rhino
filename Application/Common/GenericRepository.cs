using Application.Database;
using Application.Utility;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities.BaseEntity;

namespace Application.Common
{
    public class GenericRepository<T> :IGenericRepository<T> where T : class
    {
        protected readonly IApplicationDataContext Context;

        public GenericRepository(IApplicationDataContext context)
        {
            Context = context;
            Entities = Context.Set<T>(); // City => Cities
        }

        public async Task<bool> AddAsync(T Entity)
        {
            try
            {
                await Context.Set<T>().AddAsync(Entity);
                await Context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {

                return false;
            }
        }

        public async Task<bool> AddRangeAsync(List<T> Entities)
        {
            try
            {
                await Context.Set<T>().AddRangeAsync(Entities);
                return true;
            }
            catch (Exception ex)
            {

                return false;
            }
        }
        public IQueryable<T> GetQuery(bool includeDeleted = false)
        {
            IQueryable<T> query = Context.Set<T>();

            if (!includeDeleted && typeof(ISoftDelete).IsAssignableFrom(typeof(T)))
            {
                query = query.Where(e => !((ISoftDelete)e).IsDeleted);
            }

            return query.AsQueryable();
        }


        public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken deafult) => await Context.Set<T>().ToListAsync();

        public async Task<T> GetByIdAsync(params object[] keyValues) => await Context.Set<T>().FindAsync(keyValues);

        public async Task<bool> RemoveAsync(int id)
        {
            try
            {
                var data = await GetByIdAsync(id);
                Context.Set<T>().Remove(data);
                return true;
            }
            catch (Exception ex)
            {
                return true;
            }
        }

        public bool Update(T Entity)
        {
            try
            {
                Context.Set<T>().Update(Entity);
                return true;
            }
            catch (Exception ex)
            {

                return false;
            }

        }

        public bool UpdateRangeAsync(List<T> Entities)
        {
            try
            {
                Context.Set<T>().UpdateRange(Entities);
                return true;
            }
            catch (Exception ex)
            {

                return false;
            }
        }

        public IQueryable<T> GetAsNoTrackingQuery(bool includeDeleted = false)
        {
            IQueryable<T> query = Context.Set<T>();

            if (!includeDeleted && typeof(ISoftDelete).IsAssignableFrom(typeof(T)))
            {
                query = query.Where(e => !((ISoftDelete)e).IsDeleted);
            }

            return query.AsNoTracking();
        }
        


        public DbSet<T> Entities { get; }

        #region Async Method
        public virtual ValueTask<T> GetByIdAsync(CancellationToken cancellationToken, params object[] ids)
        {
            return Entities.FindAsync(ids, cancellationToken);
        }

        public virtual async Task AddAsync(T entity, CancellationToken cancellationToken, bool saveNow = true)
        {
            Assert.NotNull(entity, nameof(entity));
            await Entities.AddAsync(entity, cancellationToken).ConfigureAwait(false);
            if (saveNow)
                await Context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public virtual async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken, bool saveNow = true)
        {
            Assert.NotNull(entities, nameof(entities));
            await Entities.AddRangeAsync(entities, cancellationToken).ConfigureAwait(false);
            if (saveNow)
                await Context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public virtual async Task UpdateAsync(T entity, CancellationToken cancellationToken, bool saveNow = true)
        {
            Assert.NotNull(entity, nameof(entity));
            Entities.Update(entity);
            if (saveNow)
                await Context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public virtual async Task UpdateRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken, bool saveNow = true)
        {
            Assert.NotNull(entities, nameof(entities));
            Entities.UpdateRange(entities);
            if (saveNow)
                await Context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public virtual async Task DeleteAsync(T entity, CancellationToken cancellationToken, bool saveNow = true)
        {
            Assert.NotNull(entity, nameof(entity));
            Entities.Remove(entity);
            if (saveNow)
                await Context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public virtual async Task DeleteRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken, bool saveNow = true)
        {
            Assert.NotNull(entities, nameof(entities));
            Entities.RemoveRange(entities);
            if (saveNow)
                await Context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        #endregion

        #region Sync Methods
        public virtual T GetById(params object[] ids)
        {
            return Entities.Find(ids);
        }

        public virtual void Add(T entity, bool saveNow = true)
        {
            Assert.NotNull(entity, nameof(entity));
            Entities.Add(entity);
            if (saveNow)
                Context.SaveChanges();
        }

        public virtual void AddRange(IEnumerable<T> entities, bool saveNow = true)
        {
            Assert.NotNull(entities, nameof(entities));
            Entities.AddRange(entities);
            if (saveNow)
                Context.SaveChanges();
        }

        public virtual void Update(T entity, bool saveNow = true)
        {
            Assert.NotNull(entity, nameof(entity));
            Entities.Update(entity);
            if (saveNow)
                Context.SaveChanges();
        }

        public virtual void UpdateRange(IEnumerable<T> entities, bool saveNow = true)
        {
            Assert.NotNull(entities, nameof(entities));
            Entities.UpdateRange(entities);
            if (saveNow)
                Context.SaveChanges();
        }

        public virtual void Delete(T entity, bool saveNow = true)
        {
            Assert.NotNull(entity, nameof(entity));
            Entities.Remove(entity);
            if (saveNow)
                Context.SaveChanges();
        }

        public virtual void DeleteRange(IEnumerable<T> entities, bool saveNow = true)
        {
            Assert.NotNull(entities, nameof(entities));
            Entities.RemoveRange(entities);
            if (saveNow)
                Context.SaveChanges();
        }
        #endregion

        #region Attach & Detach
        public virtual void Detach(T entity)
        {
            Assert.NotNull(entity, nameof(entity));
            var entry = Entities.Entry(entity);
            if (entry != null)
                entry.State = EntityState.Detached;
        }

        public virtual void Attach(T entity)
        {
            Assert.NotNull(entity, nameof(entity));
            if (Entities.Entry(entity).State == EntityState.Detached)
                Entities.Attach(entity);
        }
        #endregion

        #region Explicit Loading
        public virtual async Task LoadCollectionAsync<TProperty>(T entity, Expression<Func<T, IEnumerable<TProperty>>> collectionProperty, CancellationToken cancellationToken)
            where TProperty : class
        {
            Attach(entity);

            var collection = Entities.Entry(entity).Collection(collectionProperty);
            if (!collection.IsLoaded)
                await collection.LoadAsync(cancellationToken).ConfigureAwait(false);
        }

        public virtual void LoadCollection<TProperty>(T entity, Expression<Func<T, IEnumerable<TProperty>>> collectionProperty)
            where TProperty : class
        {
            Attach(entity);
            var collection = Entities.Entry(entity).Collection(collectionProperty);
            if (!collection.IsLoaded)
                collection.Load();
        }

        public virtual async Task LoadReferenceAsync<TProperty>(T entity, Expression<Func<T, TProperty>> referenceProperty, CancellationToken cancellationToken)
            where TProperty : class
        {
            Attach(entity);
            var reference = Entities.Entry(entity).Reference(referenceProperty);
            if (!reference.IsLoaded)
                await reference.LoadAsync(cancellationToken).ConfigureAwait(false);
        }

        public virtual void LoadReference<TProperty>(T entity, Expression<Func<T, TProperty>> referenceProperty)
            where TProperty : class
        {
            Attach(entity);
            var reference = Entities.Entry(entity).Reference(referenceProperty);
            if (!reference.IsLoaded)
                reference.Load();
        }

        public virtual async Task SoftDeleteAsync(T entity, CancellationToken cancellationToken, bool saveNow = true)
        {
            if (entity.GetType().GetInterfaces().Contains(typeof(ISoftDelete)))
            {
                (entity as ISoftDelete).IsDeleted = true;
            }
            else
            {
                throw new InvalidOperationException($"{nameof(entity)} does not have interface {nameof(ISoftDelete)}");
            }
            await UpdateAsync(entity, cancellationToken, saveNow);
        }

      
        #endregion
    }
}
