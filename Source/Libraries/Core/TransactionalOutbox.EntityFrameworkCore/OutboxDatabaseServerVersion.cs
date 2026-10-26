using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using System;

namespace TransactionalOutbox.EntityFrameworkCore
{
	/// <summary>
	/// EF Core (Pomelo): версия сервера БД для UseMySql. Определяется один раз, а не при создании каждого контекста
	/// </summary>
	internal sealed class OutboxDatabaseServerVersion
	{
		private readonly Lazy<ServerVersion> _serverVersion;

		public OutboxDatabaseServerVersion(MySqlConnectionStringBuilder connectionStringBuilder)
		{
			_serverVersion = new Lazy<ServerVersion>(() => ServerVersion.AutoDetect(connectionStringBuilder.ConnectionString));
		}

		public ServerVersion ServerVersion => _serverVersion.Value;
	}
}
