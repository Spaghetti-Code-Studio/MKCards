using Microsoft.EntityFrameworkCore;

namespace MKCards.Server.Data
{
	public interface IDbContextOptionsBuilder
	{
		Action<DbContextOptionsBuilder> GetConfiguration();
	}
}
