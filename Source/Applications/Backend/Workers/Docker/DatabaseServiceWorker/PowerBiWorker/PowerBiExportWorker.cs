using Dapper;
using DatabaseServiceWorker.Helpers;
using DatabaseServiceWorker.PowerBiWorker.Dto;
using DatabaseServiceWorker.PowerWorker.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MySqlConnector;
using QS.DomainModel.UoW;
using QS.Project.DB;
using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.EntityRepositories.Delivery;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Sale;
using Vodovoz.Infrastructure;
using Vodovoz.Settings.Common;
using Vodovoz.Settings.Delivery;
using Vodovoz.Settings.Nomenclature;
using Vodovoz.Zabbix.Sender;

namespace DatabaseServiceWorker.PowerBiWorker
{
	internal partial class PowerBiExportWorker : TimerBackgroundServiceBase
	{
		private bool _workInProgress;
		private readonly ILogger<PowerBiExportWorker> _logger;
		private readonly IOptions<PowerBiExportOptions> _options;
		private readonly IServiceScopeFactory _serviceScopeFactory;
		private readonly IDatabaseConnectionSettings _sourceDatabaseConnectionSettings;

		public PowerBiExportWorker(
			ILogger<PowerBiExportWorker> logger,
			IOptions<PowerBiExportOptions> options,
			IServiceScopeFactory serviceScopeFactory,
			IDatabaseConnectionSettings sourceDatabaseConnectionSettings)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_options = options ?? throw new ArgumentNullException(nameof(options));
			_serviceScopeFactory = serviceScopeFactory;
			_sourceDatabaseConnectionSettings = sourceDatabaseConnectionSettings ?? throw new ArgumentNullException(nameof(sourceDatabaseConnectionSettings));
			Interval = _options.Value.Interval;
			CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
		}

		protected override void OnStartService()
		{
			_logger.LogInformation(
				"Воркер {Worker} запущен в: {StartTime}",
				nameof(PowerBiExportWorker),
				DateTimeOffset.Now);

			base.OnStartService();
		}

		protected override void OnStopService()
		{
			_logger.LogInformation(
				"Воркер {Worker} завершил работу в: {StopTime}",
				nameof(PowerBiExportWorker),
				DateTimeOffset.Now);

			base.OnStopService();
		}

		protected override TimeSpan Interval { get; }

		protected override async Task DoWork(CancellationToken stoppingToken)
		{
			if(_workInProgress)
			{
				return;
			}

			_workInProgress = true;

			using var scope = _serviceScopeFactory.CreateScope();
			var connectionFactory = scope.ServiceProvider.GetRequiredService<IPowerBiConnectionFactory>();
			using var sourceConnection = connectionFactory.CreateConnection(_sourceDatabaseConnectionSettings);
			using var targetConnection = connectionFactory.CreateConnection(_options.Value.TargetDataBase);

			try
			{
				var unitOfWorkFactory = scope.ServiceProvider.GetRequiredService<IUnitOfWorkFactory>();
				var nomenclatureSettings = scope.ServiceProvider.GetRequiredService<INomenclatureSettings>();
				var deliveryRulesSettings = scope.ServiceProvider.GetRequiredService<IDeliveryRulesSettings>();
				var generalSettings = scope.ServiceProvider.GetRequiredService<IGeneralSettings>();
				var trackRepository = scope.ServiceProvider.GetRequiredService<ITrackRepository>();
				var scheduleRestrictionRepository = scope.ServiceProvider.GetRequiredService<IScheduleRestrictionRepository>();
				var deliveryRepository = scope.ServiceProvider.GetRequiredService<IDeliveryRepository>();

				_logger.LogInformation("Начало экспорта в бд PowerBi {PowerBiExportDate}", DateTime.Now);

				var lastWorkerStartDate = await GetLastWorkerStartDateAsync(targetConnection);

				if(lastWorkerStartDate.HasValue && lastWorkerStartDate >= DateTime.Today)
				{
					return;
				}
				else
				{
					await SetLastWorkerStartDate(targetConnection, DateTime.Now);
				}

				// Скорее всего не понадобится, пока низвестно
				await ExportReportsAsync(
					sourceConnection,
					targetConnection,
					unitOfWorkFactory,
					_options.Value.StartDate, // Потом заменить на GetStartDate(), если вообще понадобится
					DateTime.Now.Date,
					generalSettings,
					deliveryRepository,
					trackRepository,
					scheduleRestrictionRepository,
					nomenclatureSettings,
					deliveryRulesSettings,
					stoppingToken);

				await ExportTablesAsync(
					sourceConnection,
					targetConnection,
					GetStartDate(_options.Value.StartDate, _options.Value.NumberOfDaysToExport),					
					stoppingToken);

				var zabbixSender = scope.ServiceProvider.GetRequiredService<IZabbixSender>();
				await zabbixSender.SendIsHealthyAsync(stoppingToken);
			}
			catch(Exception e)
			{
				_logger.LogError(
					e,
					"Ошибка при при эскпорте из БД {TodayDate}",
					DateTime.Today.ToString("dd-MM-yyyy"));
			}
			finally
			{
				_workInProgress = false;

				await sourceConnection?.CloseAsync();
				await targetConnection?.CloseAsync();
			}

			_logger.LogInformation(
				"Воркер {WorkerName} ожидает '{DelayInMinutes}' перед следующим запуском", nameof(PowerBiExportWorker), Interval);

			await Task.CompletedTask;
		}

		private async Task SetLastWorkerStartDate(MySqlConnection targetConnection, DateTime date)
		{
			var updateSettingsScript = $"update settings set value = '{date}' where name = 'last_worker_start_date';";
			await targetConnection.ExecuteAsync(updateSettingsScript);
		}

		private async Task<DateTime?> GetLastWorkerStartDateAsync(MySqlConnection targetConnection)
		{
			var sql = @$"select value from settings where name = 'last_worker_start_date';";

			return (await targetConnection.GetDataAsync<DateTime>(sql)).SingleOrDefault();
		}
	}
}
