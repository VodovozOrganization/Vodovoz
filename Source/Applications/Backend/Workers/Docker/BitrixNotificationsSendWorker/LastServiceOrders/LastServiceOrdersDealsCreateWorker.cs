using BitrixNotificationsSend.Library.Options;
using BitrixNotificationsSendWorker.PlannedOrders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Infrastructure;
using Vodovoz.Zabbix.Sender;

namespace BitrixNotificationsSendWorker.LastServiceOrders
{
	/// <summary>
	/// Воркер создания сделок по последним сервисным заказам клиентов в Битрикс24
	/// </summary>
	public class LastServiceOrdersDealsCreateWorker : TimerBackgroundServiceBase
	{
		private readonly ILogger<LastServiceOrdersDealsCreateWorker> _logger;
		private readonly IOptions<LastServiceOrdersDealsCreateOptions> _options;
		private readonly IServiceScopeFactory _serviceScopeFactory;
		private readonly IZabbixSender _zabbixSender;

		public LastServiceOrdersDealsCreateWorker(
			ILogger<LastServiceOrdersDealsCreateWorker> logger,
			IOptions<LastServiceOrdersDealsCreateOptions> options,
			IServiceScopeFactory serviceScopeFactory,
			IZabbixSender zabbixSender)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_options = options;
			_serviceScopeFactory = serviceScopeFactory;
			_zabbixSender = zabbixSender ?? throw new ArgumentNullException(nameof(zabbixSender));
		}

		protected override TimeSpan Interval => _options.Value.Interval;

		protected override Task DoWork(CancellationToken stoppingToken)
		{
			throw new NotImplementedException();
		}

		private bool IsInSendTimeInterval(DateTime moscowNow) =>
			moscowNow.TimeOfDay >= _options.Value.SendTimeFrom
			&& moscowNow.TimeOfDay < _options.Value.SendTimeTo;
	}
}
