using Microsoft.EntityFrameworkCore;

namespace MKCards.Server.Data
{
	/// <summary>
	/// Enables certain database functionalies such as logging of sensitive data.
	/// Never enable this in production unless you're debugging a critical issue and understand the consequences.
	/// May leak passwords, emails, or private info into logs.
	/// Allow it only when developing.
	/// </summary>
	public struct DatabaseDebugMode
	{
		private readonly bool _value = false;

		public DatabaseDebugMode(bool value)
		{
			_value = value;
		}

		public static DatabaseDebugMode True => new DatabaseDebugMode(true);
		public static DatabaseDebugMode False => new DatabaseDebugMode(false);

		public static implicit operator bool(DatabaseDebugMode mode) => mode._value;
	}

	public class MySqlDbContextOptionsBuilder : IDbContextOptionsBuilder
	{
		string ConnectionString { get; set; } = string.Empty;
		Version ServerVersion { get; set; } = new Version(1, 1, 1);
		DatabaseDebugMode DebugMode { get; set; } = DatabaseDebugMode.False;

		public MySqlDbContextOptionsBuilder(string connectionString, Version serverVersion)
		{
			ConnectionString = connectionString;
			ServerVersion = serverVersion;
			DebugMode = DatabaseDebugMode.False;
		}

		public MySqlDbContextOptionsBuilder(string connectionString, Version serverVersion, DatabaseDebugMode debugMode)
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
