using System;
using Notebook.DataService.IRepository;

namespace Notebook.DataService.IConfiguration
{
	public interface IUnitofWork
	{
		IUserRepository Users { get; }

		IRefreshTokenRepository RefreshToken { get; }

        Task CompleteAsync();
	}
}

