using Dapper;
using DatabaseServiceWorker.PowerBiWorker.Extensions;
using DatabaseServiceWorker.PowerBiWorker.Factories;
using DatabaseServiceWorker.PowerBiWorker.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MySqlConnector;
using QS.Project.DB;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DatabaseServiceWorker.PowerBiWorker.Exporters
{
	public class PowerBiExporter : IPowerBiExporter
	{
		private const int _timeOut = 1000;

		private readonly IPowerBiConnectionFactory _connectionFactory;
		private readonly IDatabaseConnectionSettings _sourceDatabaseConnectionSettings;
		private readonly ILogger<PowerBiExporter> _logger;
		private MySqlConnection _sourceConnection;
		private MySqlConnection _targetConnection;
		private DateTime _exportFromDate;
		private IDatabaseConnectionSettings _targetDataBaseConnectionSettings;

		public PowerBiExporter(
			IPowerBiConnectionFactory connectionFactory,
			IOptions<PowerBiExportOptions> options,
			IDatabaseConnectionSettings sourceDatabaseConnectionSettings,
			ILogger<PowerBiExporter> logger)
		{
			if(options is null)
			{
				throw new ArgumentNullException(nameof(options));
			}

			_connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
			_sourceDatabaseConnectionSettings = sourceDatabaseConnectionSettings ?? throw new ArgumentNullException(nameof(sourceDatabaseConnectionSettings));
			_targetDataBaseConnectionSettings = options.Value.TargetDataBaseConnectionSettings;
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_exportFromDate = GetStartDate(options.Value.StartDate, options.Value.NumberOfDaysToExport);
		}

		public async Task Export(CancellationToken cancellationToken)
		{
			_sourceConnection = _connectionFactory.CreateConnection(_sourceDatabaseConnectionSettings);
			_targetConnection = _connectionFactory.CreateConnection(_targetDataBaseConnectionSettings);

			try
			{
				var lastWorkerStartDate = await GetLastWorkerStartDateAsync();

				var needExport = lastWorkerStartDate.HasValue
								 && lastWorkerStartDate.Value.Date < DateTime.Today;

				if(!needExport)
				{
					_logger.LogInformation("Экспорт уже производился сегодня в бд PowerBi {PowerBiExportDate}", DateTime.Now);
				}
				else
				{
					await SetLastWorkerStartDateAsNow();
				}

				await CreateCalendarDateAsync(); //Пока Пётр разбирается как получить даты на стороне PowerBi

				var tablesToExport = await _targetConnection.GetDataAsync<dynamic>("select name, date_column_name from tables;");

				var tablesFromId = tablesToExport.Where(x => string.IsNullOrEmpty(x.date_column_name)).ToList();

				await ExportTablesFromMaxId(tablesFromId, cancellationToken);

				await ExportTablesFromDate(cancellationToken);
			}
			finally
			{
				await _sourceConnection?.CloseAsync();
				await _targetConnection?.CloseAsync();
			}
		}

		private async Task ExportTablesFromDate(CancellationToken cancellationToken)
		{
			_logger.LogInformation("Экспорт таблиц с очисткой...");

			var ordersTransaction = await _targetConnection.BeginTransactionAsync(cancellationToken);

			#region OrderItems

			var orderItemsSelectDeleteSql = @"oi.* from order_items oi left join orders o on o.id = oi.order_id 
											where o.create_date > @date or oi.order_id is null or o.id is null;";

			var orderItemsInsertSql = await GetInsertSql("order_items", ordersTransaction, cancellationToken);

			await ReExportInTransactionAsync(orderItemsSelectDeleteSql, orderItemsInsertSql, ordersTransaction, cancellationToken);

			#endregion OrderItems

			#region Orders

			var ordersSelectDeleteSql = @"o.* from orders o where create_date > @date;";

			var ordersInsertSql = await GetInsertSql("orders", ordersTransaction, cancellationToken);

			await ReExportInTransactionAsync(ordersSelectDeleteSql, ordersInsertSql, ordersTransaction, cancellationToken);

			#endregion Orders

			await ordersTransaction.CommitAsync(cancellationToken);
			await ordersTransaction.DisposeAsync();

			var undeliveryTransaction = await _targetConnection.BeginTransactionAsync(cancellationToken);

			#region Guilty

			var guiltySelectDeleteSql = @"g.* from guilty_in_undelivered_orders g left join undelivered_orders u on u.id = g.undelivery_id
				where u.creation_date > @date or g.undelivery_id is null or u.id is null;";

			var guiltyInsertSql = await GetInsertSql("guilty_in_undelivered_orders", undeliveryTransaction, cancellationToken);

			await ReExportInTransactionAsync(guiltySelectDeleteSql, guiltyInsertSql, undeliveryTransaction, cancellationToken);

			#endregion Guilty

			#region UndeliveredOrders

			var undeliveriesSelectDeleteSql = @"u.* from undelivered_orders u where creation_date > @date;";

			var undeliveriesInsertSql = await GetInsertSql("undelivered_orders", undeliveryTransaction, cancellationToken);

			await ReExportInTransactionAsync(undeliveriesSelectDeleteSql, undeliveriesInsertSql, undeliveryTransaction, cancellationToken);

			#endregion UndeliveredOrders

			await undeliveryTransaction.CommitAsync(cancellationToken);
			await undeliveryTransaction.DisposeAsync();
		}

		private async Task ReExportInTransactionAsync(string selectDeleteSql, string insertSql, IDbTransaction transaction, CancellationToken cancellationToken)
		{
			var list = await _sourceConnection.GetDataAsync<dynamic>($"select {selectDeleteSql}", new { date = _exportFromDate });

			_logger.LogInformation($"Получили {list.Count} строк для повторного экспорта с даты {_exportFromDate}: {selectDeleteSql}");

			await _targetConnection.ExecuteAsync($"delete {selectDeleteSql}", new { date = _exportFromDate }, transaction, _timeOut);

			await _targetConnection.ExecuteAsync(insertSql, list, transaction, _timeOut);
		}

		private async Task ExportTablesFromMaxId(IList<dynamic> tablesToExport, CancellationToken cancellationToken)
		{
			_logger.LogInformation("Получено {TablesCount} таблиц для экспорта", tablesToExport.Count);

			foreach(var table in tablesToExport)
			{
				await ExportTableFromMaxId(table.name, cancellationToken);
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

		private async Task ExportTableFromMaxId(string tableName, CancellationToken cancellationToken)
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

				using var transaction = await _targetConnection.BeginTransactionAsync(cancellationToken);

				var insertSql = await GetInsertSql(tableName, transaction, cancellationToken);
				await _targetConnection.ExecuteAsync(insertSql, list, transaction, _timeOut);
				await transaction.CommitAsync(cancellationToken);
				await transaction.DisposeAsync();

				_logger.LogInformation("Успешно экспортировали {RecordsCount} записей в таблицу {TableName}", rowsToExportCount, tableName);
			}
		}

		private async Task<string> GetInsertSql(string tableName, IDbTransaction transaction, CancellationToken cancellationToken)
		{
			var columns = await _targetConnection.GetDataAsync<string>(
				$"SELECT column_name FROM INFORMATION_SCHEMA.COLUMNS WHERE table_name = '{tableName}';", transaction: transaction);

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

			var dateString = (await _targetConnection.GetDataAsync<string>(sql)).SingleOrDefault();

			var result = DateTime.Parse(dateString, CultureInfo.GetCultureInfo("ru-RU"));

			return result;
		}

		private DateTime GetStartDate(DateTime optionsStartDate, int numberOfDaysToUpdate)
		{
			var updateDate = DateTime.Today.AddDays(-numberOfDaysToUpdate);
			var startDate = optionsStartDate > updateDate ? optionsStartDate : updateDate;
			return startDate;
		}
	}
}
