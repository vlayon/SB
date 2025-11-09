using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Data.Context;

namespace Data.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly SnippingBotDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(SnippingBotDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public async Task<T> AddAsync(T entity)
        {
            _dbSet.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public Task<T?> GetAsync(Expression<Func<T, bool>> predicate) =>
            _dbSet.AsNoTracking().FirstOrDefaultAsync(predicate);

        public Task<List<T>> ListAsync(Expression<Func<T, bool>>? predicate = null) =>
            predicate == null ? _dbSet.ToListAsync() : _dbSet.Where(predicate).ToListAsync();

        public Task<bool> AnyAsync(Expression<Func<T, bool>> predicate) =>
            _dbSet.AnyAsync(predicate);

        public Task SaveChangesAsync() => _context.SaveChangesAsync();
    }
}