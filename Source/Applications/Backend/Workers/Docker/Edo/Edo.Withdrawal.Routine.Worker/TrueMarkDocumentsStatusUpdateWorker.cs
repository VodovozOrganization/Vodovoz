using Edo.Withdrawal.Routine.Options;
using Edo.Withdrawal.Routine.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Infrastructure;
using Vodovoz.Zabbix.Sender;

namespace Edo.Withdrawal.Routine.Worker
{
	/// <summary>
	/// Воркер для обновления статусов документов ЧЗ
	/// </summary>
	public class TrueMarkDocumentsStatusUpdateWorker : TimerBackgroundServiceBase
	{
		private readonly ILogger<TrueMarkDocumentsStatusUpdateWorker> _logger;
		private readonly IOptions<WithdrawalRoutineOptions> _options;
		private readonly IServiceScopeFactory _serviceScopeFactory;

		public TrueMarkDocumentsStatusUpdateWorker(
			ILogger<TrueMarkDocumentsStatusUpdateWorker> logger,
			IOptions<WithdrawalRoutineOptions> options,
			IServiceScopeFactory serviceScopeFactory)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_options = options;
			_serviceScopeFactory = serviceScopeFactory;
		}

		protected override TimeSpan Interval => _options.Value.TrueMarkDocumentsStatusUpdateWorkerInterval;

		protected override async Task DoWork(CancellationToken stoppingToken)
		{
			_logger.LogInformation("Начало обновления статусов документов ЧЗ");

			using var scope = _serviceScopeFactory.CreateScope();
			var zabbixSender = scope.ServiceProvider.GetService<IZabbixSender>();
			var documentStatusUpdateService = scope.ServiceProvider.GetService<TrueMarkDocumentsStatusUpdateService>();

			try
			{
				await documentStatusUpdateService.UpdateTrueMarkDocuments(stoppingToken);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка при выполнении обновления статусов документов ЧЗ");
			}

			await zabbixSender.SendIsHealthyAsync(stoppingToken);

			_logger.LogInformation(
				"Обновление статусов документов ЧЗ успешно завершена");
		}
	}
}
