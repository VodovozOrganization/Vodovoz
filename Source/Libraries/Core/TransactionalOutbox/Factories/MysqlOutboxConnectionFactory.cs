using MySqlConnector;
using System.Data.Common;
using TransactionalOutbox.Abstractions;

namespace TransactionalOutbox.Factories
{
	/// <summary>
	/// Фабрика для создания соединений с MySQL базой данных для работы с транзакционным аутбоксом.
	/// </summary>
	public class MysqlOutboxConnectionFactory : IOutboxConnectionFactory
	{
		private readonly string _connectionString;

		public MysqlOutboxConnectionFactory(DbConnectionStringBuilder connectionStringBuilder)
		{
			_connectionString = connectionStringBuilder.ConnectionString;
		}

		public DbConnection CreateConnection()
		{
			return new MySqlConnection(_connectionString);
		}
	}
}
