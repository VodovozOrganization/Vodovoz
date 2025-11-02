using Microsoft.Extensions.Logging;
using NHibernate;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Vodovoz.Infrastructure;
using Vodovoz.Settings.Edo;

namespace Edo.Transfer.Routine
{
	public class TransferTimeoutWorker : TimerBackgroundServiceBase
	{
		private readonly ILogger<TransferTimeoutWorker> _logger;
		private readonly IEdoTransferSettings _edoTransferSettings;
		private readonly IServiceScopeFactory _serviceScopeFactory;
		private readonly TimeSpan _transferTaskTimeoutCheckIntervalSecond;

		public TransferTimeoutWorker(
			ILogger<TransferTimeoutWorker> logger,
			IEdoTransferSettings edoTransferSettings,
			IServiceScopeFactory serviceScopeFactory
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_edoTransferSettings = edoTransferSettings ?? throw new ArgumentNullException(nameof(edoTransferSettings));
			_serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));

			_transferTaskTimeoutCheckIntervalSecond = TimeSpan.FromSeconds(_edoTransferSettings.TransferTaskRequestsWaitingTimeoutCheckIntervalSecond);
		}

		protected override TimeSpan Interval => _transferTaskTimeoutCheckIntervalSecond;

		protected override async Task DoWork(CancellationToken stoppingToken)
		{
			try
			{
				using(var scope = _serviceScopeFactory.CreateScope())
				{
					var staleTransferSender = scope.ServiceProvider.GetService<StaleTransferSender>();
					await staleTransferSender.SendStaleTasksAsync(stoppingToken);
				}
			}
			catch(StaleObjectStateException ex)
			{
				_logger.LogInformation("Задача уже была кем-то изменена. Попытка закрытия задачи будет произведена на следующей итерации воркера");
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка при периодической проверки задач трансфера на необходимость отправки.");
			}
		}
	}
}
