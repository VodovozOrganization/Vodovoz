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
	/// Воркер отправки документов отмены вывода из оборота в ЧЗ перед переотправкой УПД.
	/// </summary>
	public class TrueMarkWithdrawalCancellationWorker : TimerBackgroundServiceBase
	{
		private readonly ILogger<TrueMarkWithdrawalCancellationWorker> _logger;
		private readonly IOptions<WithdrawalRoutineOptions> _options;
		private readonly IServiceScopeFactory _serviceScopeFactory;

		public TrueMarkWithdrawalCancellationWorker(
			ILogger<TrueMarkWithdrawalCancellationWorker> logger,
			IOptions<WithdrawalRoutineOptions> options,
			IServiceScopeFactory serviceScopeFactory)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_options = options ?? throw new ArgumentNullException(nameof(options));
			_serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
		}

		/// <summary>
		/// Интервал выполнения воркера.
		/// </summary>
		protected override TimeSpan Interval => _options.Value.TrueMarkDocumentsStatusUpdateWorkerInterval;

		/// <summary>
		/// Выполнить отправку документов отмены вывода из оборота в ЧЗ.
		/// </summary>
		/// <param name="stoppingToken">Токен остановки</param>
		protected override async Task DoWork(CancellationToken stoppingToken)
		{
			_logger.LogInformation("Начало отправки документов отмены вывода из оборота в ЧЗ");

			using var scope = _serviceScopeFactory.CreateScope();
			var zabbixSender = scope.ServiceProvider.GetService<IZabbixSender>();
			var cancellationService = scope.ServiceProvider.GetService<ITrueMarkWithdrawalCancellationService>();

			try
			{
				await cancellationService.SendCancellationDocuments(stoppingToken);
				await cancellationService.PublishReadyResendRequests(stoppingToken);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка при отправке документов отмены вывода из оборота в ЧЗ");
			}

			await zabbixSender.SendIsHealthyAsync(nameof(TrueMarkWithdrawalCancellationWorker), stoppingToken);

			_logger.LogInformation("Отправка документов отмены вывода из оборота в ЧЗ завершена");
		}
	}
}
