using Microsoft.Extensions.Configuration;
using MySqlConnector;
using NLog;
using Polly;
using System;
using System.Collections.Concurrent;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using VodovozMangoService.Calling;
using VodovozMangoService.Extensions;

namespace VodovozMangoService.Services
{
	public class CallerService : ICallerService
	{
		private readonly ConcurrentDictionary<string, CallerInfoCache> _externalCallers;
		//private readonly ILogger<CallerService> _logger;
		private readonly Logger _logger = LogManager.GetCurrentClassLogger();
		private readonly string _connectionString;

		public CallerService(
			//ILogger<CallerService> logger,
			IConfiguration configuration)
		{
			_externalCallers = new ConcurrentDictionary<string, CallerInfoCache>();
			//_logger = logger ?? throw new ArgumentNullException(nameof(logger));

			var connectionStringBuilder = new MySqlConnectionStringBuilder();
			connectionStringBuilder.Server = configuration["Mysql:mysql_server_host_name"];
			connectionStringBuilder.Port = uint.Parse(configuration["Mysql:mysql_server_port"]);
			connectionStringBuilder.Database = configuration["Mysql:mysql_database"]; ;
			connectionStringBuilder.UserID = configuration["Mysql:mysql_user"]; ;
			connectionStringBuilder.Password = configuration["Mysql:mysql_password"]; ;
			connectionStringBuilder.SslMode = MySqlSslMode.Disabled;
			connectionStringBuilder.DefaultCommandTimeout = 5;
			_connectionString = connectionStringBuilder.ConnectionString;
		}

		public Task RemoveOutDated()
		{
			var outdatedCachedValues = _externalCallers.Where((kvPair) => kvPair.Value.LiveTime.TotalMinutes > 5);

			foreach(var outdatedValue in outdatedCachedValues)
			{
				if(!_externalCallers.TryRemove(outdatedValue.Key, out var deletedPair))
				{
					//_logger.LogWarning("Не удалось удалить устаревший номер {Number}", outdatedValue.Key);
					_logger.Warn("Не удалось удалить устаревший номер {Number}", outdatedValue.Key);
				}
			}

			return Task.CompletedTask;
		}

		public async Task<Caller> GetExternalCaller(string number)
		{
			Caller result = new Caller();

			try
			{
				await RemoveOutDated();

				if(_externalCallers.TryGetValue(number, out var cachedValue))
				{
					return cachedValue.Caller;
				}

				//_logger.LogDebug("Поиск телефона: {Number}...", number);
				_logger.Debug("Поиск телефона: {Number}...", number);

				CallerInfoCache callerInfoCache;

				var digits = number.Substring(number.Length - Math.Min(10, number.Length));
				var sql =
					"SELECT IFNULL(client_counterparty.name, delivery_point_counterparty.name) as counterparty_name, " +
					"delivery_points.compiled_address_short as address, CONCAT_WS(\" \", employees.last_name, employees.name, employees.patronymic) as employee_name, " +
					"phones.employee_id, phones.delivery_point_id, IFNULL(client_counterparty.id, delivery_point_counterparty.id)  as counterparty_id, " +
					"subdivisions.short_name as subdivision_name " +
					"FROM phones " +
					"LEFT JOIN employees ON employees.id = phones.employee_id " +
					"LEFT JOIN subdivisions ON subdivisions.id = employees.subdivision_id " +
					"LEFT JOIN delivery_points ON delivery_points.id = phones.delivery_point_id " +
					"LEFT JOIN counterparty as client_counterparty ON client_counterparty.id = phones.counterparty_id " +
					"LEFT JOIN counterparty as delivery_point_counterparty ON delivery_point_counterparty.id = delivery_points.counterparty_id " +
					"WHERE phones.digits_number = @digits;";

				var retryPolicy = Policy
					.Handle<MySqlException>()
					.Or<TimeoutException>()
					.WaitAndRetryAsync(1, (_) => TimeSpan.FromSeconds(1));

				using var connection = new MySqlConnection(_connectionString);

				var list = (await connection.QueryAsyncWithRetry<PhoneWithDetailsResponseNode>(sql, retryPolicy, new { digits })).ToList();

				//_logger.LogDebug("{PhonesCount} телефонов в базе данных.", list.Count);
				_logger.Debug("{PhonesCount} телефонов в базе данных.", list.Count);

				//Очищаем контрагентов у которых номер соответсвует звонящей точке доставки
				list.RemoveAll(x => !string.IsNullOrEmpty(x.counterparty_name) && string.IsNullOrEmpty(x.address)
					&& list.Any(a => !string.IsNullOrEmpty(a.counterparty_name) && !string.IsNullOrEmpty(a.address)));

				callerInfoCache = new CallerInfoCache(new Caller
				{
					Number = number,
					Type = CallerType.External,
				});

				foreach(var row in list)
				{
					callerInfoCache.Caller.Names.Add(new CallerName
					{
						Name = TitleExternalName(row),
						CounterpartyId = (uint?)row.counterparty_id ?? 0,
						DeliveryPointId = (uint?)row.delivery_point_id ?? 0,
						EmployeeId = (uint?)row.employee_id ?? 0
					});
				}

				_externalCallers.TryAdd(number, callerInfoCache);

				result = callerInfoCache.Caller;
			}
			catch(Exception e)
			{
				//_logger.LogError(e, "Ошибка при выполнении запроса поиска контрагента по номеру телефона: {Message}.", e.Message);
				_logger.Error(e, "Ошибка при выполнении запроса поиска контрагента по номеру телефона: {Message}.", e.Message);
			}

			return result;
		}

		private string TitleExternalName(dynamic row)
		{
			if(!string.IsNullOrWhiteSpace(row.employee_name))
			{
				return row.subdivision_name == null ? row.employee_name : $"{row.employee_name} ({row.subdivision_name})";
			}

			if(!string.IsNullOrWhiteSpace(row.address))
			{
				return $"{row.counterparty_name} ({row.address})";
			}

			return row.counterparty_name ?? string.Empty;
		}

		private class PhoneWithDetailsResponseNode
		{
			public string counterparty_name { get; set; }
			public string address { get; set; }
			public string employee_name { get; set; }
			public int employee_id { get; set; }
			public int delivery_point_id { get;set; }
			public int counterparty_id { get; set; }
			public string subdivision_name { get; set; }
		}
	}
}
