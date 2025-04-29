using InternetCafe.Domain.Entities;
using InternetCafe.Domain.Enums;
using InternetCafe.Application.Interfaces.Repositories;
using InternetCafe.Infrastructure.DBContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace InternetCafe.Infrastructure.Repositories
{
    public class GenericRepository<T> : IRepository<T> where T : BaseEntity
    {
        protected readonly ApplicationDbContext _dbContext;
        protected readonly DbSet<T> _dbSet;

        public GenericRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _dbSet = dbContext.Set<T>();
        }

        public async Task<T?> GetByIdAsync(int id)
        {
            var entity = await _dbSet.FindAsync(id);
            return entity != null && entity.Status == (int)Status.Active ? entity : null;
        }

        public async Task<IReadOnlyList<T>> GetAllAsync()
        {
            return await _dbSet.Where(e => e.Status == (int)Status.Active).ToListAsync();
        }

        public async Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            var parameter = Expression.Parameter(typeof(T), "x");
            var statusProperty = Expression.Property(parameter, "Status");
            var statusEqualsActive = Expression.Equal(statusProperty, Expression.Constant(Status.Active));

            var originalBody = predicate.Body;
            var combinedBody = Expression.AndAlso(statusEqualsActive, originalBody);

            var newPredicate = Expression.Lambda<Func<T, bool>>(
                combinedBody,
                predicate.Parameters
            );

            return await _dbSet.Where(newPredicate).ToListAsync();
        }

        public async Task<T> AddAsync(T entity)
        {
            entity.Status = (int)Status.Active;
            await _dbSet.AddAsync(entity);
            return entity;
        }

        public Task UpdateAsync(T entity)
        {
            _dbContext.Entry(entity).State = EntityState.Modified;
            return Task.CompletedTask;
        }

        public Task DeleteAsync(T entity)
        {
            entity.Status = (int)Status.Cancelled;
            _dbContext.Entry(entity).State = EntityState.Modified;
            return Task.CompletedTask;
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _dbContext.SaveChangesAsync();
        }

        public async Task<IReadOnlyList<T>> GetAllIncludingCancelledAsync()
        {
            return await _dbSet.ToListAsync();
        }
    }
}