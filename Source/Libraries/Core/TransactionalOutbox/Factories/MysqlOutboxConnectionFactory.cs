using MySqlConnector;
using System.Data.Common;
using TransactionalOutbox.Abstractions;

namespace TransactionalOutbox.Factories
{
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
