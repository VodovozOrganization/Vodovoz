using Edo.Withdrawal.Routine.Options;
using Edo.Withdrawal.Routine.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Infrastructure;

namespace Edo.Withdrawal.Routine
{
	/// <summary>
	/// Воркер для автоматического вывода кодов из оборота для клиентов,
	/// подключённых к ЭДО и ЧЗ, но не принимающих документы в течение N дней
	/// </summary>
	public class TrueMarkTimedOutDocumentsWithdrawalWorker : TimerBackgroundServiceBase
	{
		private readonly ILogger<TrueMarkTimedOutDocumentsWithdrawalWorker> _logger;
		private readonly IOptions<WithdrawalRoutineOptions> _options;
		private readonly IServiceScopeFactory _serviceScopeFactory;

		public TrueMarkTimedOutDocumentsWithdrawalWorker(
			ILogger<TrueMarkTimedOutDocumentsWithdrawalWorker> logger,
			IOptions<WithdrawalRoutineOptions> options,
			IServiceScopeFactory serviceScopeFactory)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_options = options;
			_serviceScopeFactory = serviceScopeFactory;
		}

		/// <summary>
		/// Интервал выполнения воркера
		/// </summary>
		protected override TimeSpan Interval => _options.Value.Interval;

		/// <summary>
		/// Выполнить проверку и создать заявки на вывод из оборота для просроченных документооборотов
		/// </summary>
		/// <param name="stoppingToken">Токен остановки</param>
		protected override async Task DoWork(CancellationToken stoppingToken)
		{
			try
			{
				_logger.LogInformation("Начало обработки просроченных документооборотов для вывода кодов из оборота");

				using(var scope = _serviceScopeFactory.CreateScope())
				{
					var docflowTimeoutCheckService = scope.ServiceProvider.GetService<TrueMarkTimedOutDocumentsWithdrawalService>();

					await docflowTimeoutCheckService.ProcessTimedOutDocumentTasks(stoppingToken);

					_logger.LogInformation(
						"Обработка просроченных документооборотов для вывода кодов из оборота успешно завершена");
				}
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка при выполнении обработки вывода кодов из оборота");
			}
		}
	}
}
