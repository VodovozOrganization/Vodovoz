using Edo.Transfer.Routine.Options;
using Edo.Transfer.Routine.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Infrastructure;

namespace Edo.Transfer.Routine
{
	internal class ClosingDocumentsOrdersUpdSendWorker : TimerBackgroundServiceBase
	{
		private readonly ILogger<ClosingDocumentsOrdersUpdSendWorker> _logger;
		private readonly IOptionsMonitor<ClosingDocumentsOrdersUpdSendSettings> _optionsMonitor;
		private readonly IServiceScopeFactory _serviceScopeFactory;

		public ClosingDocumentsOrdersUpdSendWorker(
			ILogger<ClosingDocumentsOrdersUpdSendWorker> logger,
			IOptionsMonitor<ClosingDocumentsOrdersUpdSendSettings> optionsMonitor,
			IServiceScopeFactory serviceScopeFactory)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_optionsMonitor = optionsMonitor;
			_serviceScopeFactory = serviceScopeFactory;
		}

		protected override TimeSpan Interval => _optionsMonitor.CurrentValue.Interval;

		protected override async Task DoWork(CancellationToken stoppingToken)
		{
			using(var scope = _serviceScopeFactory.CreateScope())
			{
				var closingDocumentsOrdersUpdSendService = scope.ServiceProvider.GetService<ClosingDocumentsOrdersUpdSendService>();

				_logger.LogInformation("Start closing documents orders upd send");

				try
				{
					await closingDocumentsOrdersUpdSendService.Send(stoppingToken);
				}
				catch(Exception ex)
				{
					_logger.LogError(ex, "Error while sending closing documents orders upd");
				}

				_logger.LogInformation("End closing documents orders upd send");
			}
		}
	}
}
