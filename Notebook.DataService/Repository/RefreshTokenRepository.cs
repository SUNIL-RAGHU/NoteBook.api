using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Notebook.DataService.Data;
using Notebook.DataService.IRepository;
using Notebook.Entities.DbSet;

namespace Notebook.DataService.Repository
{
    public class RefreshTokenRepository : GenericRepository<RefreshToken>, IRefreshTokenRepository
    {
        public RefreshTokenRepository(AppDbContext Context,ILogger logger) : base(Context,logger)
        {
        }


        public override async Task<IEnumerable<RefreshToken>> All()
        {
            try
            {
                return await dbset.Where(x => x.Status == 1).AsNoTracking().ToListAsync();
            }

            catch(Exception ex)
            {
                _logger.LogError(ex,"{Repo} All method has generated an error",typeof(RefreshTokenRepository));
                return new List<RefreshToken>();
            }
        }

        public async Task<RefreshToken> GetByRefreshToken(string refreshToken)
        {
            try
            {
                return await dbset.Where(x => x.Token.ToLower() == refreshToken.ToLower()).AsNoTracking().FirstOrDefaultAsync();
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "{Repo} GetByRefreshToken Method has been generated an erro", typeof(RefreshTokenRepository));
                return null;
            }
        }

        public async Task<bool> MarkRefreshTokenAsUsed(RefreshToken refreshToken)
        {
            try
            {
                var token =await dbset.Where(x => x.Token.ToLower() == refreshToken.ToLower()).AsNoTracking().FirstOrDefaultAsync();
                if (token != null)
                {
                    return false;
                }

                token.IsUsed = refreshToken.IsUsed;
                return true;
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "{Repo} MarkRefreshTokenAsUsed  Method has been generated an erro", typeof(RefreshTokenRepository));
                return false;
            }
        }
    }
}

    