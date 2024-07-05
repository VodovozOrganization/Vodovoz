using ClosedXML.Excel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QS.DomainModel.UoW;
using SharpCifs.Smb;
using System;
using System.IO;
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

namespace DatabaseServiceWorker
{
	internal partial class PowerBiExportWorker : TimerBackgroundServiceBase
	{
		private bool _workInProgress;
		private readonly ILogger<PowerBiExportWorker> _logger;
		private readonly IOptions<PowerBiExportOptions> _options;
		private readonly IServiceScopeFactory _serviceScopeFactory;

		public PowerBiExportWorker(
			ILogger<PowerBiExportWorker> logger,
			IOptions<PowerBiExportOptions> options,
			IServiceScopeFactory serviceScopeFactory)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_options = options ?? throw new ArgumentNullException(nameof(options));
			_serviceScopeFactory = serviceScopeFactory;
			Interval = _options.Value.Interval;
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

			try
			{
				var unitOfWorkFactory = scope.ServiceProvider.GetRequiredService<IUnitOfWorkFactory>();
				var nomenclatureSettings = scope.ServiceProvider.GetRequiredService<INomenclatureSettings>();
				var deliveryRulesSettings = scope.ServiceProvider.GetRequiredService<IDeliveryRulesSettings>();
				var generalSettings = scope.ServiceProvider.GetRequiredService<IGeneralSettings>();
				var trackRepository = scope.ServiceProvider.GetRequiredService<ITrackRepository>();
				var scheduleRestrictionRepository = scope.ServiceProvider.GetRequiredService<IScheduleRestrictionRepository>();
				var deliveryRepository = scope.ServiceProvider.GetRequiredService<IDeliveryRepository>();

				ReadFromDbAndExportToFile(
					unitOfWorkFactory,
					generalSettings,
					trackRepository,
					scheduleRestrictionRepository,
					deliveryRepository,
					deliveryRulesSettings,
					nomenclatureSettings,
					stoppingToken);
				
				var zabbixSender = scope.ServiceProvider.GetRequiredService<IZabbixSender>();
				await zabbixSender.SendIsHealthyAsync();
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
			}

			_logger.LogInformation(
				"Воркер {WorkerName} ожидает '{DelayInMinutes}' перед следующим запуском", nameof(PowerBiExportWorker), Interval);

			await Task.CompletedTask;
		}

		private void ReadFromDbAndExportToFile(
			IUnitOfWorkFactory unitOfWorkFactory,
			IGeneralSettings generalSettings,
			ITrackRepository trackRepository,
			IScheduleRestrictionRepository scheduleRestrictionRepository,
			IDeliveryRepository deliveryRepository,
			IDeliveryRulesSettings deliveryRulesSettings,
			INomenclatureSettings nomenclatureSettings,
			CancellationToken stoppingToken)
		{
			var smbPath = $"smb://{_options.Value.Login}:{_options.Value.Password}@{_options.Value.ExportPath}";

			var file = new SmbFile(smbPath);
			var readStream = file.GetInputStream();
			var memStream = new MemoryStream();
			((Stream)readStream).CopyTo(memStream);
			readStream.Dispose();

			using(var excelWorkbook = new XLWorkbook(memStream))
			{
				if(IsNeedExportToday(excelWorkbook))
				{
					ClearSheetsData(excelWorkbook);

					using(var uow = unitOfWorkFactory.CreateWithoutRoot(nameof(PowerBiExportWorker)))
					{
						for(DateTime date = _options.Value.StartDate; date < DateTime.Now.Date; date = date.AddDays(1))
						{
							ReadDataFromDbAndExportToExcel(
								uow,
								excelWorkbook,
								date,
								generalSettings,
								deliveryRepository,
								trackRepository,
								scheduleRestrictionRepository,
								nomenclatureSettings,
								deliveryRulesSettings,
								stoppingToken);
						}
					}

					excelWorkbook.Save();
					WriteExcelStreamToFile(file, memStream);
				}
			}

			memStream.Dispose();
		}
	}
}
