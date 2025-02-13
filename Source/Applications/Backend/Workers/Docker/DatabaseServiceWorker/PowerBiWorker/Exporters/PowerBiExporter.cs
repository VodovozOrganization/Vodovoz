using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using DatabaseServiceWorker.PowerBiWorker.Extensions;
using DatabaseServiceWorker.PowerBiWorker.Factories;
using DatabaseServiceWorker.PowerBiWorker.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MySqlConnector;
using QS.Project.DB;

namespace DatabaseServiceWorker.PowerBiWorker.Exporters
{
	public class PowerBiExporter : IPowerBiExporter
	{
		private const int _timeOut = 1000;

		private readonly IPowerBiConnectionFactory _connectionFactory;
		private readonly IOptions<PowerBiExportOptions> _options;
		private readonly IDatabaseConnectionSettings _sourceDatabaseConnectionSettings;
		private readonly ILogger<PowerBiExporter> _logger;
		private MySqlConnection _sourceConnection;
		private MySqlConnection _targetConnection;

		public PowerBiExporter(
			IPowerBiConnectionFactory connectionFactory,
			IOptions<PowerBiExportOptions> options,
			IDatabaseConnectionSettings sourceDatabaseConnectionSettings,
			ILogger<PowerBiExporter> logger)
		{
			_connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
			_options = options ?? throw new ArgumentNullException(nameof(options));
			_sourceDatabaseConnectionSettings = sourceDatabaseConnectionSettings ??
												throw new ArgumentNullException(nameof(sourceDatabaseConnectionSettings));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public async Task Export(CancellationToken cancellationToken)
		{
			_sourceConnection = _connectionFactory.CreateConnection(_sourceDatabaseConnectionSettings);
			_targetConnection = _connectionFactory.CreateConnection(_options.Value.TargetDataBaseConnectionSettings);

			try
			{
				var lastWorkerStartDate = await GetLastWorkerStartDateAsync();

				var needExport = lastWorkerStartDate.HasValue
								 && DateTime.Now - lastWorkerStartDate.Value >= TimeSpan.FromHours(1)
								 && DateTime.Now.Minute is >= 30;

				if(!needExport)
				{
					_logger.LogInformation("Экспорт уже производился в текущем часе в бд PowerBi {PowerBiExportDate}", DateTime.Now);

					return;
				}
				else
				{
					await SetLastWorkerStartDateAsNow();
				}

				await TruncateTables(cancellationToken);

				await ExportTables(cancellationToken);
			}
			finally
			{
				await _sourceConnection?.CloseAsync();
				await _targetConnection?.CloseAsync();
			}
		}

		private async Task ExportTables(CancellationToken cancellationToken)
		{
			await CreateCalendarDateAsync(); //Пока Пётр разбирается как получить даты на стороне PowerBi

			var tablesToExport = await _targetConnection.GetDataAsync<string>("select name from tables");
			_logger.LogInformation("Получено {TablesCount} таблиц для экспорта", tablesToExport.Count);

			foreach(var table in tablesToExport)
			{
				await ExportTable(table, cancellationToken);
			}
		}

		private async Task CreateCalendarDateAsync()
		{
			var maxCalendarDateInTarget = (await _targetConnection.GetDataAsync<DateTime>("select max(date) from calendar;")).Single();

			if(maxCalendarDateInTarget >= DateTime.Today.AddDays(-1))
			{
				return;
			}

			var ruCulture = CultureInfo.CreateSpecificCulture("ru-RU");
			var date = DateTime.Today.AddDays(-1);

			var montNum = date.Month;
			var monthName = ruCulture.TextInfo.ToTitleCase(date.ToString("MMMM", ruCulture));
			var monthShortName = ruCulture.TextInfo.ToTitleCase(date.ToString("MMM", ruCulture));

			var weekDayNum = date.DayOfWeek == 0 ? 7 : ((int)date.DayOfWeek);
			var weekDayName = ruCulture.TextInfo.ToTitleCase(date.ToString("dddd", ruCulture));
			var weekDayShortName = ruCulture.TextInfo.ToTitleCase(date.ToString("ddd", ruCulture));

			var dayNum = date.Day;
			var isWorkDay = weekDayNum < 6;

			var year = date.Year;

			var calendarInsertSql = $@"INSERT INTO calendar (`date`, month_num, month_name, month_shortName, weekday_num, weekday_name, weekday_shortName, day_num, workday, year)
						VALUES(""{date.ToString("yyyy-MM-dd")}"", {montNum}, ""{monthName}"", ""{monthShortName}"", {weekDayNum}, ""{weekDayName}"", ""{weekDayShortName}"", {dayNum}, {isWorkDay}, {year})";


			_targetConnection.Execute(calendarInsertSql, commandTimeout: _timeOut);
		}

		private async Task ExportTable(string tableName, CancellationToken cancellationToken)
		{
			var maxIdSql = GetMaxIdFromTableSql(tableName);
			var maxInTarget = (await _targetConnection.GetDataAsync<int>(maxIdSql)).Single();
			var maxInSource = (await _sourceConnection.GetDataAsync<int>(maxIdSql)).Single();

			var rowsToExportCount = maxInSource - maxInTarget;

			if(rowsToExportCount > 0)
			{
				_logger.LogInformation("Начинаем экспорт {RecordsCount} записей в таблицу {TableName}", rowsToExportCount, tableName);

				var sql = GetSelectAllGreatThanIdSql(tableName, maxInTarget);
				var list = await _sourceConnection.GetDataAsync<dynamic>(sql, new { id = maxInTarget });

				var insertSql = await GetInsertSql(tableName, cancellationToken);

				using var transaction = await _targetConnection.BeginTransactionAsync(cancellationToken);
				await _targetConnection.ExecuteAsync(insertSql, list, transaction, _timeOut);
				await transaction.CommitAsync(cancellationToken);

				_logger.LogInformation("Успешно экспортировали {RecordsCount} записей в таблицу {TableName}", rowsToExportCount, tableName);
			}
		}

		private async Task<string> GetInsertSql(string tableName, CancellationToken cancellationToken)
		{
			var columns = await _targetConnection.GetDataAsync<string>(
				$"SELECT column_name FROM INFORMATION_SCHEMA.COLUMNS WHERE table_name = '{tableName}';");

			var columNames = string.Join(", ", columns);
			var parametersNames = string.Join(", ", columns.Select(col => $"@{col}"));

			return $"INSERT INTO {tableName} ({columNames}) VALUES ({parametersNames});";
		}

		private string GetMaxIdFromTableSql(string tableName) => $"select max(id) from {tableName};";

		private string GetSelectAllGreatThanIdSql(string tableName, int id) => $"select * from {tableName} where id > {id};";

		private async Task SetLastWorkerStartDateAsNow()
		{
			var ruDateTimeFormat = CultureInfo.GetCultureInfo("ru-RU").DateTimeFormat;
			var updateSettingsScript = $"update settings set value = '{DateTime.Now.ToString(ruDateTimeFormat)}' where name = 'last_worker_start_date';";
			await _targetConnection.ExecuteAsync(updateSettingsScript);
		}

		private async Task<DateTime?> GetLastWorkerStartDateAsync()
		{
			var sql = @$"select value from settings where name = 'last_worker_start_date';";

			var dateString =  (await _targetConnection.GetDataAsync<string>(sql)).SingleOrDefault();

			var result = DateTime.Parse(dateString, CultureInfo.GetCultureInfo("ru-RU"));

			return result;
		}

		private DateTime GetTruncateStartDate(DateTime optionsStartDate, int numberOfDaysToUpdate)
		{
			var updateDate = DateTime.Today.AddDays(-numberOfDaysToUpdate);
			var startDate = optionsStartDate > updateDate ? optionsStartDate : updateDate;
			return startDate;
		}

		private async Task TruncateTables(CancellationToken cancellationToken)
		{
			_logger.LogInformation("Начинием очистку необходимых таблиц");

			var truncateFromDate = GetTruncateStartDate(_options.Value.StartDate, _options.Value.NumberOfDaysToExport);

			using var truncateTransaction = await _targetConnection.BeginTransactionAsync(cancellationToken);

			await _targetConnection.ExecuteAsync(
				@"delete g.* from guilty_in_undelivered_orders g left join undelivered_orders u on u.id = g.undelivery_id
									 where u.creation_date > @date or g.undelivery_id is null or u.id is null;",
				new { date = truncateFromDate }, truncateTransaction, _timeOut);

			await _targetConnection.ExecuteAsync("delete from undelivered_orders where creation_date > @date;",
				new { date = truncateFromDate },
				truncateTransaction, _timeOut);

			await _targetConnection.ExecuteAsync("delete oi.* from order_items oi left join orders o on o.id = oi.order_id" +
												 " where o.create_date > @date or oi.order_id is null or o.id is null;",
				new { date = truncateFromDate }, truncateTransaction, _timeOut);

			await _targetConnection.ExecuteAsync("delete from orders where create_date > @date;", new { date = truncateFromDate },
				truncateTransaction,
				_timeOut);

			await truncateTransaction.CommitAsync(cancellationToken);

			_logger.LogInformation("Успешно очисти ли необходимые таблицы");
		}
	}
}
