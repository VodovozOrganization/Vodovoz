using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Mindbox.Library.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Mindbox.CounterpartySender
{
	/// <summary>
	/// Публикует новых контрагентов в outbox Mindbox.
	/// Отправку сообщений в Mindbox выполняет потребитель MassTransit.
	/// </summary>
	public class CounterpartySenderWorker : BackgroundService
	{
		private const int DelaySeconds = 10;

		private readonly ILogger<CounterpartySenderWorker> _logger;
		private readonly IServiceScopeFactory _scopeFactory;

		public CounterpartySenderWorker(
			ILogger<CounterpartySenderWorker> logger,
			IServiceScopeFactory scopeFactory)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
		}

		/// <inheritdoc />
		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			_logger.LogInformation("Воркер отправки контрагентов в Mindbox запущен");

			while(!stoppingToken.IsCancellationRequested)
			{
				try
				{
					using var scope = _scopeFactory.CreateScope();
					var enqueuer = scope.ServiceProvider.GetRequiredService<IMindboxCounterpartyEnqueuer>();
					var enqueued = await enqueuer.EnqueueNewCounterpartiesAsync(stoppingToken);

					if(enqueued > 0)
					{
						_logger.LogInformation("Добавлено в очередь Mindbox: {Count}", enqueued);
					}
				}
				catch(OperationCanceledException)
				{
					break;
				}
				catch(Exception ex)
				{
					_logger.LogError(ex, "Ошибка постановки контрагентов в очередь Mindbox");
				}

				try
				{
					await Task.Delay(TimeSpan.FromSeconds(DelaySeconds), stoppingToken);
				}
				catch(OperationCanceledException)
				{
					break;
				}
			}
		}
	}
}
