using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Caribbean.DataContexts.Application;

namespace Caribbean.DataAccessLayer.Database
{
    public interface IGenericRepository<TEntity> where TEntity : class
    {
        Task<IEnumerable<TEntity>> Get(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            string includeProperties = "");
        Task<TEntity> GetById(object id);
        Task<TEntity> GetSingle(Expression<Func<TEntity, bool>> filter, string includeProperties = "");
        void Add(TEntity entity);
        void Delete(object id);
        void Delete(TEntity entityToDelete);
        void Update(TEntity entityToUpdate);
    }

    public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class
    {
        protected readonly ApplicationDbContext _db;
        protected readonly DbSet<TEntity> _dbSet;

        public GenericRepository(ApplicationDbContext db)
        {
            _db = db;
            _dbSet = _db.Set<TEntity>();
        }

        public virtual async Task<IEnumerable<TEntity>> Get(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            string includeProperties = "")
        {
            IQueryable<TEntity> query = _dbSet;
            if (filter != null) query = query.Where(filter);
            foreach (var includeProperty in includeProperties.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }
            return orderBy != null ? await orderBy(query).ToListAsync() : await query.ToListAsync();
        }

        public virtual async Task<TEntity> GetById(object id)
        {
            return await _dbSet.FindAsync(id);
        }

        public virtual async Task<TEntity> GetSingle(Expression<Func<TEntity, bool>> filter, string includeProperties = "")
        {
            IQueryable<TEntity> query = _dbSet;
            if (filter != null) query = query.Where(filter);
            foreach (var includeProperty in includeProperties.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }
            return await query.SingleOrDefaultAsync();
        }

        public virtual void Add(TEntity entity)
        {
            _dbSet.Add(entity);
        }

        public virtual void Delete(object id)
        {
            var entityToDelete = _dbSet.Find(id);
            Delete(entityToDelete);
        }

        public virtual void Delete(TEntity entityToDelete)
        {
            if (_db.Entry(entityToDelete).State == EntityState.Detached)
            {
                _dbSet.Attach(entityToDelete);
            }
            _dbSet.Remove(entityToDelete);
        }

        public virtual void Update(TEntity entityToUpdate)
        {
            _dbSet.Attach(entityToUpdate);
            _db.Entry(entityToUpdate).State = EntityState.Modified;
        }
    }
}
