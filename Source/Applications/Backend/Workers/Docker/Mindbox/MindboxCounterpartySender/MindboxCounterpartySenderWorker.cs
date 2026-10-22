using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Vodovoz.Mindbox.Services;

namespace MindboxCounterpartySender
{
	/// <summary>
	/// Отправляет очередь контрагентов в Mindbox
	/// </summary>
	public class MindboxCounterpartySenderWorker : BackgroundService
	{
		private const int DelaySeconds = 10;

		private readonly ILogger<MindboxCounterpartySenderWorker> _logger;
		private readonly IServiceScopeFactory _scopeFactory;

		public MindboxCounterpartySenderWorker(
			ILogger<MindboxCounterpartySenderWorker> logger,
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
					using(var scope = _scopeFactory.CreateScope())
					{
						var processor = scope.ServiceProvider.GetRequiredService<IMindboxCounterpartyOutboxProcessor>();
						var processed = await processor.ProcessAsync(stoppingToken);

						if(processed > 0)
						{
							_logger.LogInformation("Обработано сообщений очереди Mindbox: {Count}", processed);
						}
					}
				}
				catch(OperationCanceledException)
				{
					break;
				}
				catch(Exception ex)
				{
					_logger.LogError(ex, "Ошибка обработки очереди контрагентов Mindbox");
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
