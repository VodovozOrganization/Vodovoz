using MySqlConnector;
using System;

namespace Vodovoz.Testing.Database
{
	/// <summary>
	/// Последний рубеж перед подключением: тесты работают только с сервером, который запустили сами.
	/// </summary>
	public static class SafetyGuard
	{
		public const string DatabasePrefix = "test_";
		private const string _allowedHost = "127.0.0.1";

		/// <summary>
		/// Бросает исключение, если строка подключения ведёт куда-либо, кроме локальной тестовой БД.
		/// </summary>
		/// <param name="connectionString">Строка подключения</param>
		/// <param name="allowedPort">Порт сервера, запущенного самими тестами</param>
		public static void EnsureAllowed(string connectionString, int allowedPort)
		{
			var builder = new MySqlConnectionStringBuilder(connectionString);

			if(builder.Server != _allowedHost)
			{
				throw new InvalidOperationException($"Тестам разрешён только хост {_allowedHost}, указан {builder.Server}");
			}

			if(builder.Port != allowedPort)
			{
				throw new InvalidOperationException($"Тестам разрешён только порт запущенного ими сервера {allowedPort}, указан {builder.Port}");
			}

			if(!string.IsNullOrEmpty(builder.Database)
				&& !builder.Database.StartsWith(DatabasePrefix, StringComparison.Ordinal))
			{
				throw new InvalidOperationException($"Тестам разрешены только базы с префиксом {DatabasePrefix}, указана {builder.Database}");
			}
		}
	}
}
