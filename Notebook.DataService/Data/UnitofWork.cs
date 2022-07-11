using System;
using Microsoft.Extensions.Logging;
using Notebook.DataService.IConfiguration;
using Notebook.DataService.IRepository;
using Notebook.DataService.Repository;

namespace Notebook.DataService.Data
{
	public class UnitofWork:IUnitofWork,IDisposable
	{
		private readonly AppDbContext _Context;

		private readonly ILogger _logger;

        public IUserRepository Users { get; private set; }

        public IRefreshTokenRepository RefreshToken { get; set; }
        public UnitofWork(AppDbContext context,ILoggerFactory loggerFactory)
        {
            _Context = context;
            _logger = loggerFactory.CreateLogger("db_logs");

            Users = new UsersRepository(context, _logger);
            RefreshToken = new RefreshTokenRepository(context, _logger);

        }

      

        public void Dispose()
        {
            _Context.Dispose();
        }

        public async Task CompleteAsync()
        {
           await _Context.SaveChangesAsync();
        }
    }
}

