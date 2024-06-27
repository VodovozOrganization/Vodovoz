using ClosedXML.Excel;
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

namespace DatabaseServiceWorker
{
	internal partial class PowerBiExportWorker : TimerBackgroundServiceBase
	{
		private bool _workInProgress;
		private readonly ILogger<PowerBiExportWorker> _logger;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IOptions<PowerBiExportOptions> _options;
		private readonly INomenclatureSettings _nomenclatureSettings;
		private readonly IGeneralSettings _generalSettings;
		private readonly IDeliveryRulesSettings _deliveryRulesSettings;
		private readonly IDeliveryRepository _deliveryRepository;
		private readonly ITrackRepository _trackRepository;
		private readonly IScheduleRestrictionRepository _scheduleRestrictionRepository;

		public PowerBiExportWorker(
			ILogger<PowerBiExportWorker> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			IOptions<PowerBiExportOptions> options,
			INomenclatureSettings nomenclatureSettings,
			IGeneralSettings generalSettings,
			IDeliveryRulesSettings deliveryRulesSettings,
			IDeliveryRepository deliveryRepository,
			ITrackRepository trackRepository,
			IScheduleRestrictionRepository scheduleRestrictionRepository)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_options = options ?? throw new ArgumentNullException(nameof(options));
			_nomenclatureSettings = nomenclatureSettings ?? throw new ArgumentNullException(nameof(nomenclatureSettings));
			_generalSettings = generalSettings ?? throw new ArgumentNullException(nameof(generalSettings));
			_deliveryRulesSettings = deliveryRulesSettings ?? throw new ArgumentNullException(nameof(deliveryRulesSettings));
			_deliveryRepository = deliveryRepository ?? throw new ArgumentNullException(nameof(deliveryRepository));
			_trackRepository = trackRepository ?? throw new ArgumentNullException(nameof(trackRepository));
			_scheduleRestrictionRepository = scheduleRestrictionRepository ?? throw new ArgumentNullException(nameof(scheduleRestrictionRepository));
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

			try
			{
				ReadFromDbAndExportToFile(stoppingToken);
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

		private void ReadFromDbAndExportToFile(CancellationToken stoppingToken)
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

					using(var uow = _unitOfWorkFactory.CreateWithoutRoot(nameof(PowerBiExportWorker)))
					{
						for(DateTime date = _options.Value.StartDate; date < DateTime.Now.Date; date = date.AddDays(1))
						{
							ReadDataFromDbAndExportToExcel(uow, excelWorkbook, date, _nomenclatureSettings, stoppingToken);
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
