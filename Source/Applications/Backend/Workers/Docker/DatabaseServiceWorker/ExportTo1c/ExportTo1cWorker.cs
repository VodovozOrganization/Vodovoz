using DatabaseServiceWorker.Options;
using ExportTo1c.Library.Factories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QS.DomainModel.UoW;
using SharpCifs.Smb;
using SharpCifs.Util.Sharpen;
using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Extensions;
using Vodovoz.Infrastructure;
using Vodovoz.Settings.Orders;
using Vodovoz.Zabbix.Sender;

namespace DatabaseServiceWorker.ExportTo1c
{
	internal class ExportTo1cWorker : TimerBackgroundServiceBase
	{
		private bool _workInProgress;
		private readonly ILogger<ExportTo1cWorker> _logger;
		private readonly IOptions<ExportTo1cOptions> _options;
		private readonly IZabbixSender _zabbixSender;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IOrderRepository _orderRepository;
		private readonly ICounterpartyRepository _counterpartyRepository;
		private readonly IOrderSettings _orderSettings;
		private readonly IDataExporterFor1cFactory _dataExporterFor1cFactory;
		private const string _leftPartNameOfExportFile = "ДВ-1с-обмен";

		public ExportTo1cWorker(
			ILogger<ExportTo1cWorker> logger,
			IOptions<ExportTo1cOptions> options,
			IZabbixSender zabbixSender,
			IUnitOfWorkFactory unitOfWorkFactory,
			IOrderRepository orderRepository,
			ICounterpartyRepository counterpartyRepository,
			IOrderSettings orderSettings,
			IDataExporterFor1cFactory dataExporterFor1CFactory)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_options = options ?? throw new ArgumentNullException(nameof(options));
			_zabbixSender = zabbixSender ?? throw new ArgumentNullException(nameof(zabbixSender));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_counterpartyRepository = counterpartyRepository ?? throw new ArgumentNullException(nameof(counterpartyRepository));
			_orderSettings = orderSettings ?? throw new ArgumentNullException(nameof(orderSettings));
			_dataExporterFor1cFactory = dataExporterFor1CFactory ?? throw new ArgumentNullException(nameof(dataExporterFor1CFactory));
			Interval = options.Value.ExportInterval;
		}

		protected override void OnStartService()
		{
			_logger.LogInformation(
				"Воркер {Worker} запущен в: {TransferStartTime}",
				nameof(ExportTo1cWorker),
				DateTimeOffset.Now);

			base.OnStartService();
		}

		protected override void OnStopService()
		{
			_logger.LogInformation(
				"Воркер {Worker} завершил работу в: {StopTime}",
				nameof(ExportTo1cWorker),
				DateTimeOffset.Now);

			base.OnStopService();
		}

		protected override TimeSpan Interval { get; }

		protected override async Task DoWork(CancellationToken cancellationToken)
		{
			if(_workInProgress)
			{
				return;
			}

			_workInProgress = true;

			try
			{
				_logger.LogInformation("Начало экспорта данных 1С из бд в файл {Export1cExportDate}", DateTime.Now);

				await ExportIfNeeded(cancellationToken);

				await _zabbixSender.SendIsHealthyAsync(cancellationToken);

				_logger.LogInformation("Экспорт данных 1С из бд в файл завершён.");
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Ошибка при экспорте данных 1С из бд в файл");

				await _zabbixSender.SendProblemMessageAsync(ZabixSenderMessageType.Problem, "Ошибка экспорта данных для 1c.", cancellationToken);
			}
			finally
			{
				_workInProgress = false;
			}

			_logger.LogInformation("Воркер {WorkerName} ожидает '{DelayInMinutes}' перед следующим запуском", nameof(ExportTo1cWorker), Interval);
		}

		private async Task ExportIfNeeded(CancellationToken cancellationToken)
		{
			var today = DateTime.Today;
			var yesterdayStartOfDayDate = today.AddDays(-1);
			var auth = new NtlmPasswordAuthentication("", _options.Value.Login, _options.Value.Password);
			var smbPath = $"smb://{_options.Value.ExportPath}/";

			await DeleteFilesOlderThanOneMonth(smbPath, auth, cancellationToken);

			await ExportOrders(Export1cMode.ComplexAutomation, yesterdayStartOfDayDate, smbPath, auth, cancellationToken);

			//await ExportOrders(Export1cMode.Retail, yesterdayStartOfDayDate, smbPath, auth, cancellationToken);

			await ExportCounterpartyChanges(yesterdayStartOfDayDate, smbPath, auth, cancellationToken);
		}

		private async Task ExportCounterpartyChanges(DateTime startOfDayDate, string smbPath, NtlmPasswordAuthentication auth, CancellationToken cancellationToken)
		{
			var fileName = $"{_leftPartNameOfExportFile}-ИзмененияКонтрагентов-{startOfDayDate:yyyyMMdd}.xml";

			var smbFile = new SmbFile($"{smbPath}{fileName}", auth);

			if(smbFile.Exists())
			{
				return;
			}
			
			using var unitOfWork = _unitOfWorkFactory.CreateWithoutRoot("Экспорт контрагентов для 1С");

			var endOfDayDate = startOfDayDate.AddDays(1).AddTicks(-1);

			var counterpartyChanges = _counterpartyRepository.GetCounterpartyChanges(unitOfWork, startOfDayDate, endOfDayDate);

			if(!counterpartyChanges.Any())
			{
				return;
			}
			
			var exporter = _dataExporterFor1cFactory.CreateCounterpartyChanges1cDataExporter();
			
			var xml = exporter.CreateXml(counterpartyChanges, cancellationToken);

			await ExportToFile(smbFile, xml, cancellationToken);
		}

		private async Task ExportOrders(Export1cMode exportMode, DateTime yesterday, string smbPath, NtlmPasswordAuthentication auth, CancellationToken cancellationToken)
		{
			var startOfYesterday = yesterday;
			var endOfYesterday = yesterday.AddDays(1).AddTicks(-1);

			using var unitOfWork = _unitOfWorkFactory.CreateWithoutRoot("Экспорт заказов для 1С");

			var orders = _orderRepository.GetOrdersToExport1c8(
				unitOfWork,
				_orderSettings,
				exportMode,
				startOfYesterday,
				endOfYesterday);

			foreach(var groupedOrder in orders.GroupBy(x => x.Contract.Organization))
			{
				var fileName = $"{_leftPartNameOfExportFile}-{exportMode.GetEnumDisplayName()}-{groupedOrder.Key.INN}-{yesterday:yyyyMMdd}.xml";

				var smbFile = new SmbFile($"{smbPath}{fileName}", auth);

				if(smbFile.Exists())
				{
					continue;
				}

				var organizaionOrders = groupedOrder.ToArray();

				var exporter = _dataExporterFor1cFactory.CreateOrders1cDataExporter(exportMode, groupedOrder.Key, startOfYesterday,endOfYesterday);

				var xml = exporter.CreateXml(organizaionOrders, cancellationToken);

				await ExportToFile(smbFile, xml, cancellationToken);
			}
		}


		private async Task ExportToFile(SmbFile smbFile, XElement xml, CancellationToken cancellationToken)
		{
			var settings = new XmlWriterSettings
			{
				OmitXmlDeclaration = true,
				Indent = true,
				Encoding = Encoding.UTF8,
				NewLineChars = "\r\n",
				Async = true
			};

			using var smbStream = new SmbFileOutputStream(smbFile);

			await using var writer = XmlWriter.Create(smbStream, settings);

			await xml.WriteToAsync(writer, cancellationToken);
		}

		private async Task DeleteFilesOlderThanOneMonth(string smbPath, NtlmPasswordAuthentication auth, CancellationToken cancellationToken)
		{
			try
			{
				var dir = new SmbFile(smbPath, auth);

				if(!dir.IsDirectory())
				{
					_logger.LogError("Указанный путь не является папкой");

					return;
				}

				var oneMonthAgoMs = DateTime.Now.AddMonths(-1).ToUniversalTime().ToMillisecondsSinceEpoch();

				var oldFiles = (await dir.ListFilesAsync())
					.Where(file =>
						file.IsFile()
						&& file.GetName().StartsWith(_leftPartNameOfExportFile)
						&& file.GetName().EndsWith(".xml", StringComparison.OrdinalIgnoreCase)
						&& file.CreateTime() < oneMonthAgoMs
					);

				foreach(var file in oldFiles)
				{
					if(cancellationToken.IsCancellationRequested)
					{
						return;
					}

					_logger.LogInformation($"Удаление: {file.GetName()} (Дата создания: {file.CreateTime()})");

					await file.DeleteAsync();
				}

				_logger.LogInformation("Очистка завершена");
			}
			catch(Exception ex)
			{
				_logger.LogError($"Ошибка при очистке папки: {ex.Message}");
			}
		}
	}
}
