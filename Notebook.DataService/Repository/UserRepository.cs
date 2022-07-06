using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Notebook.DataService.Data;
using Notebook.DataService.IRepository;
using Notebook.Entities.DbSet;

namespace Notebook.DataService.Repository
{
    public class UsersRepository : GenericRepository<User>, IUserRepository
    {
        public UsersRepository(AppDbContext Context,ILogger logger) : base(Context,logger)
        {
        }


        public override async Task<IEnumerable<User>> All()
        {
            try
            {
                return await dbset.Where(x => x.Status == 1).AsNoTracking().ToListAsync();
            }

            catch(Exception ex)
            {
                _logger.LogError(ex,"{Repo} All method has generated an error",typeof(UsersRepository));
                return new List<User>();
            }
        }

        
    }
}

    