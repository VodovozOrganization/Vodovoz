using Edo.Receipt.Sender;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Infrastructure;

namespace Edo.Receipt.Sender.Worker
{
	public class ReceiptCorrectionSendWorker : TimerBackgroundServiceBase
	{
		private readonly ILogger<ReceiptCorrectionSendWorker> _logger;
		private readonly IServiceScopeFactory _serviceScopeFactory;

		public ReceiptCorrectionSendWorker(
			ILogger<ReceiptCorrectionSendWorker> logger,
			IServiceScopeFactory serviceScopeFactory)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
		}

		protected override TimeSpan Interval => TimeSpan.FromSeconds(30);

		protected override async Task DoWork(CancellationToken stoppingToken)
		{
			using var scope = _serviceScopeFactory.CreateScope();
			var sender = scope.ServiceProvider.GetRequiredService<ReceiptCorrectionSender>();

			try
			{
				await sender.ProcessActiveCorrections(stoppingToken);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка цикла отправки корректирующих чеков");
			}
		}
	}
}
