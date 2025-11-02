using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using EdoAutoSendReceiveWorker.Configs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TaxcomEdo.Client;
using Vodovoz.Zabbix.Sender;

namespace EdoAutoSendReceiveWorker
{
	public class TaxcomEdoAutoSendReceiveWorker : BackgroundService
	{
		private readonly ILogger<TaxcomEdoAutoSendReceiveWorker> _logger;
		private readonly IZabbixSender _zabbixSender;
		private readonly IServiceScopeFactory _serviceScopeFactory;
		private readonly TaxcomEdoAutoSendReceiveWorkerOptions _workerOptions;

		public TaxcomEdoAutoSendReceiveWorker(
			ILogger<TaxcomEdoAutoSendReceiveWorker> logger,
			IOptions<TaxcomEdoAutoSendReceiveWorkerOptions> workerOptions,
			IZabbixSender zabbixSender,
			IServiceScopeFactory serviceScopeFactory)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_zabbixSender = zabbixSender ?? throw new ArgumentNullException(nameof(zabbixSender));
			_serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
			_workerOptions = (workerOptions ?? throw new ArgumentNullException(nameof(workerOptions))).Value;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			_logger.LogInformation("Процесс автоматизации выполнения транзакций запущен");
			await StartWorkingAsync(stoppingToken);
		}

		private async Task StartWorkingAsync(CancellationToken stoppingToken)
		{
			while(!stoppingToken.IsCancellationRequested)
			{
				try
				{
					var delay = _workerOptions.DelayBetweenAutoSendReceiveProcessingInSeconds;
					
					_logger.LogInformation("Пауза перед запуском транзакций {Delay}сек", delay);
					await Task.Delay(delay * 1000, stoppingToken);
					
					_logger.LogInformation("Отправляем запрос на запуск необходимых транзакций");
					using var scope = _serviceScopeFactory.CreateScope();
					var taxcomClient = scope.ServiceProvider.GetService<ITaxcomApiClient>();
					await taxcomClient.StartProcessAutoSendReceive(stoppingToken);
					await _zabbixSender.SendIsHealthyAsync(stoppingToken);
				}
				catch(Exception e)
				{
					_logger.LogError(e, "Ошибка при отправке запроса на запуск необходимых транзакций");
				}
			}
		}
	}
}
