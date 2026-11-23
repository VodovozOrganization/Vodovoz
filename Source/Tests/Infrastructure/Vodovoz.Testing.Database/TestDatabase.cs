using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Data;

namespace Vodovoz.Testing.Database
{
	/// <summary>
	/// Отдельная база test_&lt;имя&gt;_&lt;guid&gt; на запущенном тестами сервере. Удаляется вместе с объектом.
	/// </summary>
	public sealed class TestDatabase : IDisposable
	{
		private TestDatabase(string name)
		{
			Name = name;
			ConnectionString = MariaDbServer.Instance.GetConnectionString(name);
		}

		public string Name { get; }

		public string ConnectionString { get; }

		public static TestDatabase Create(string name)
		{
			var databaseName = $"{SafetyGuard.DatabasePrefix}{name}_{Guid.NewGuid():N}";

			Execute(MariaDbServer.Instance.GetConnectionString(), $"CREATE DATABASE `{databaseName}`");

			return new TestDatabase(databaseName);
		}

		/// <summary>
		/// Новое соединение мимо NHibernate, например для проверки того, что видят другие сессии.
		/// </summary>
		public MySqlConnection OpenConnection()
		{
			SafetyGuard.EnsureAllowed(ConnectionString, MariaDbServer.Instance.Port);

			var connection = new MySqlConnection(ConnectionString);
			connection.Open();

			return connection;
		}

		/// <summary>
		/// Очищает все таблицы базы. Изоляция тестов не откатом, потому что тестам нужны настоящие коммиты.
		/// </summary>
		public void TruncateAll()
		{
			using(var connection = OpenConnection())
			{
				var tables = new List<string>();

				using(var command = connection.CreateCommand())
				{
					command.CommandText = "SHOW TABLES";

					using(var reader = command.ExecuteReader())
					{
						while(reader.Read())
						{
							tables.Add(reader.GetString(0));
						}
					}
				}

				Execute(connection, "SET FOREIGN_KEY_CHECKS = 0");

				foreach(var table in tables)
				{
					Execute(connection, $"TRUNCATE TABLE `{table}`");
				}

				Execute(connection, "SET FOREIGN_KEY_CHECKS = 1");
			}
		}

		public long Count(string table, string where = null)
		{
			using(var connection = OpenConnection())
			{
				return Scalar(connection, IsolationLevel.ReadCommitted, $"SELECT COUNT(*) FROM `{table}`" + (where == null ? string.Empty : $" WHERE {where}"));
			}
		}

		public static long Scalar(MySqlConnection connection, IsolationLevel isolationLevel, string sql)
		{
			using(var transaction = connection.BeginTransaction(isolationLevel))
			using(var command = connection.CreateCommand())
			{
				command.Transaction = transaction;
				command.CommandText = sql;

				return Convert.ToInt64(command.ExecuteScalar());
			}
		}

		public void Execute(string sql)
		{
			using(var connection = OpenConnection())
			{
				Execute(connection, sql);
			}
		}

		public void Dispose()
		{
			MySqlConnection.ClearAllPools();
			Execute(MariaDbServer.Instance.GetConnectionString(), $"DROP DATABASE IF EXISTS `{Name}`");
		}

		private static void Execute(string connectionString, string sql)
		{
			using(var connection = new MySqlConnection(connectionString))
			{
				connection.Open();
				Execute(connection, sql);
			}
		}

		private static void Execute(MySqlConnection connection, string sql)
		{
			using(var command = connection.CreateCommand())
			{
				command.CommandText = sql;
				command.ExecuteNonQuery();
			}
		}
	}
}
