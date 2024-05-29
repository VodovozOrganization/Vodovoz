using ClosedXML.Excel;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QS.DomainModel.UoW;
using SharpCifs.Smb;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Infrastructure;
using Vodovoz.Zabbix.Sender;

namespace DatabaseServiceWorker
{
	internal partial class PowerBiExportWorker : TimerBackgroundServiceBase
	{
		private bool _workInProgress;
		private readonly ILogger<PowerBiExportWorker> _logger;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IOptions<PowerBiExportOptions> _options;
		private readonly IZabbixSender _zabbixSender;

		public PowerBiExportWorker(
			ILogger<PowerBiExportWorker> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			IOptions<PowerBiExportOptions> options,
			IZabbixSender zabbixSender)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_options = options ?? throw new ArgumentNullException(nameof(options));
			_zabbixSender = zabbixSender ?? throw new ArgumentNullException(nameof(zabbixSender));
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
				ReadFromDbAndExportToFile();
				await _zabbixSender.SendIsHealthyAsync(true);
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

		private void ReadFromDbAndExportToFile()
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
							ReadDataFromDbAndExportToExcel(uow, excelWorkbook, date);
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
