using DatabaseServiceWorker.Options;
using ExportTo1c.Library.Factories;
using ExportTo1c.Library.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QS.DomainModel.UoW;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Infrastructure;
using Vodovoz.Zabbix.Sender;


namespace DatabaseServiceWorker.ExportTo1c
{
	public class ExportTo1cApiWorker : TimerBackgroundServiceBase
	{
		private readonly ILogger<ExportTo1cApiWorker> _logger;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IApi1cChangesExporterFactory _api1cChangesExporterFactory;
		private readonly IOrderTo1cExportRepository _orderTo1CExportRepository;
		private readonly IZabbixSender _zabbixSender;
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly IOptions<ExportTo1cApiOptions> _options;

		public ExportTo1cApiWorker(
			ILogger<ExportTo1cApiWorker> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			IApi1cChangesExporterFactory api1cChangesExporterFactory,
			IOrderTo1cExportRepository orderTo1CExportRepository,
			IZabbixSender zabbixSender,
			IHttpClientFactory httpClientFactory,
			IOptions<ExportTo1cApiOptions> options)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_api1cChangesExporterFactory = api1cChangesExporterFactory ?? throw new ArgumentNullException(nameof(api1cChangesExporterFactory));
			_orderTo1CExportRepository = orderTo1CExportRepository ?? throw new ArgumentNullException(nameof(orderTo1CExportRepository));
			_zabbixSender = zabbixSender ?? throw new ArgumentNullException(nameof(zabbixSender));
			_httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
			_options = options ?? throw new ArgumentNullException(nameof(options));
			Interval = options.Value.ExportInterval;
		}

		protected override async Task DoWork(CancellationToken cancellationToken)
		{
			try
			{
				await ExportTo1cApiAsync(cancellationToken);

				await _zabbixSender.SendIsHealthyAsync(cancellationToken);
			}
			catch(Exception ex)
			{
				await _zabbixSender.SendProblemMessageAsync(ZabixSenderMessageType.Problem, ex.ToString(), cancellationToken);
			}
		}

		private async Task ExportTo1cApiAsync(CancellationToken cancellationToken)
		{
			var nowExportDate = DateTime.Now;

			var from = nowExportDate.Date.AddHours(_options.Value.DoExportFromHour);
			var to = nowExportDate.Date.AddHours(_options.Value.DoExportToHour);

			if(nowExportDate < from || nowExportDate > to)
			{
				return;
			}

			using var unitOfWork = _unitOfWorkFactory.CreateWithoutRoot(nameof(ExportTo1cApiWorker));

			var lastExportDate = await _orderTo1CExportRepository.GetMaxLastExportDate(unitOfWork, cancellationToken);

			if(lastExportDate.HasValue && lastExportDate.Value.Date == nowExportDate.Date)
			{
				return;
			}

			var changedOrders = await _orderTo1CExportRepository.GetNewChangedOrdersForExportTo1cApi(unitOfWork, cancellationToken);

			if(changedOrders.Count == 0)
			{
				return;
			}

			var exporter = _api1cChangesExporterFactory.CreateApi1cDataExporter(nowExportDate);

			var xml = exporter.CreateXml(changedOrders, cancellationToken);

			using var content = new StringContent(
					xml.ToString(),
					System.Text.Encoding.UTF8,
					"application/xml");

			var httpClient = _httpClientFactory.CreateClient();

			using var request = new HttpRequestMessage(HttpMethod.Post, _options.Value.CashlessChangesApiUri)
			{
				Content = content
			};

			using var response = await httpClient.SendAsync(request, cancellationToken);

			var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

			if(!response.IsSuccessStatusCode)
			{
				return;
			}

			foreach(var changerOrder in changedOrders)
			{
				changerOrder.LastExportDate = nowExportDate;
				changerOrder.Error = null;
				await unitOfWork.SaveAsync(changerOrder, cancellationToken: cancellationToken);
			}

			await unitOfWork.CommitAsync(cancellationToken);

			return;
		}

		protected override TimeSpan Interval { get; }

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
	}
}
