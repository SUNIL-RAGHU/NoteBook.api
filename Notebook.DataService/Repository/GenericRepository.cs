using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Notebook.DataService.Data;
using Notebook.DataService.IRepository;

namespace Notebook.DataService.Repository
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {

        protected AppDbContext _Context;

        internal DbSet<T> dbset;

        protected readonly ILogger _logger;



        public GenericRepository(AppDbContext Context,ILogger logger)
        {
            _Context = Context;
            _logger = logger;
            dbset = Context.Set<T>();
        }
        public virtual async Task<bool> Add(T entity)
        {
           await dbset.AddAsync(entity);
            return true;
        }

        public virtual async Task<IEnumerable<T>> All()
        {
           return await dbset.ToListAsync();
        }

        public virtual Task<bool> Delete(Guid id, string userId)
        {
            throw new NotImplementedException();
        }

        public virtual async Task<T> GetbyId(Guid id)
        {
            return await dbset.FindAsync(id);
        }

        public Task<bool> Upsert(T entity)
        {
            throw new NotImplementedException();
        }
    }
}

