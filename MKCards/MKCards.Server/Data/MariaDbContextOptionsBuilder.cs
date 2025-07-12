using Microsoft.EntityFrameworkCore;

namespace MKCards.Server.Data
{
	public class MariaDbContextOptionsBuilder : IDbContextOptionsBuilder
	{
		string ConnectionString { get; set; } = string.Empty;
		Version ServerVersion { get; set; } = new Version(1, 1, 1);

		/// <summary>
		/// Enables certain functionalies such as logging of sensitive data.
		/// Never enable this property in production unless you're debugging a critical issue and understand the consequences.
		/// May leak passwords, emails, or private info into logs.
		/// Allow it only when developing.
		/// </summary>
		bool DebugMode { get; set; } = false;

		public MariaDbContextOptionsBuilder(string connectionString, Version serverVersion, bool debugMode = false)
		{
			ConnectionString = connectionString;
			ServerVersion = serverVersion;
			DebugMode = debugMode;
		}

		// TODO: we might need to allow better logging in the future via e.g. LogTo(Console.WriteLine, LogLevel.Information). The question is where to log. Client actually includes Logger, but Server does not.
		public Action<DbContextOptionsBuilder> GetConfiguration()
		{
			return options =>
			{
				options.UseMySql(ConnectionString, new MySqlServerVersion(ServerVersion));
				if (DebugMode)
				{
					options.EnableSensitiveDataLogging();
					options.EnableDetailedErrors();
				}
			};
		}
	}
}
