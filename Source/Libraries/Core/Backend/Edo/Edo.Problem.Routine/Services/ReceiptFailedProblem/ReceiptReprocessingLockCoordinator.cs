using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Edo.Problem.Routine.Services.ReceiptFailedProblem
{
	/// <summary>
	/// Координатор выполнения регламентной переобработки чеков с ошибками в рамках процесса
	/// </summary>
	public class ReceiptReprocessingLockCoordinator : IReceiptReprocessingLockCoordinator
	{
		private readonly ILogger<ReceiptReprocessingLockCoordinator> _logger;
		private int _isRunning;

		/// <summary>
		/// Конструктор координатора блокировок
		/// </summary>
		/// <param name="logger">Логгер</param>
		public ReceiptReprocessingLockCoordinator(ILogger<ReceiptReprocessingLockCoordinator> logger)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		/// <inheritdoc/>
		public Task<bool> TryAcquireLockAsync(CancellationToken cancellationToken = default)
		{
			if(Interlocked.CompareExchange(ref _isRunning, 1, 0) != 0)
			{
				_logger.LogInformation("Блокировка переобработки чеков уже занята другим процессом или задачей");
				return Task.FromResult(false);
			}

			_logger.LogInformation("Успешно захвачена блокировка переобработки чеков");
			return Task.FromResult(true);
		}

		/// <inheritdoc/>
		public Task ReleaseLockAsync()
		{
			Interlocked.Exchange(ref _isRunning, 0);
			_logger.LogInformation("Блокировка переобработки чеков освобождена");
			return Task.CompletedTask;
		}
	}
}
