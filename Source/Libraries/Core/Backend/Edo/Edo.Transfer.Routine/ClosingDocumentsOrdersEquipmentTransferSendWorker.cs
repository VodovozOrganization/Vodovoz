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
	public class ClosingDocumentsOrdersEquipmentTransferSendWorker : TimerBackgroundServiceBase
	{
		private readonly ILogger<ClosingDocumentsOrdersEquipmentTransferSendWorker> _logger;
		private readonly IOptionsMonitor<ClosingDocumentsOrdersUpdSendSettings> _optionsMonitor;
		private readonly IServiceScopeFactory _serviceScopeFactory;

		public ClosingDocumentsOrdersEquipmentTransferSendWorker(
			ILogger<ClosingDocumentsOrdersEquipmentTransferSendWorker> logger,
			IOptionsMonitor<ClosingDocumentsOrdersUpdSendSettings> optionsMonitor,
			IServiceScopeFactory serviceScopeFactory
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_optionsMonitor = optionsMonitor ?? throw new ArgumentNullException(nameof(optionsMonitor));
			_serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
		}
		protected override TimeSpan Interval => _optionsMonitor.CurrentValue.Interval;

		protected override async Task DoWork(CancellationToken stoppingToken)
		{
			using(var scope = _serviceScopeFactory.CreateScope())
			{
				var closingDocumentsOrdersEquipmentSendService = scope.ServiceProvider.GetService<ClosingDocumentsOrdersEquipmentTransferSendService>();

				_logger.LogInformation("Start closing documents orders equipment transfer send");

				try
				{
					await closingDocumentsOrdersEquipmentSendService.Send(stoppingToken);
				}
				catch(Exception ex)
				{
					_logger.LogError(ex, "Error while sending closing documents orders equipment transfer");
				}

				_logger.LogInformation("End closing documents orders equipment transfer send");
			}
		}
	}
}
