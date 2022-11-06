using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Taxcom.Client.Api;

namespace TaxcomEdoApi
{
	public class AutoSendReceiveService : BackgroundService
	{
		private readonly ILogger<AutoSendReceiveService> _logger;
		private readonly TaxcomApi _taxcomApi;
		private const int _delaySec = 60;

		public AutoSendReceiveService(ILogger<AutoSendReceiveService> logger, TaxcomApi taxcomApi)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_taxcomApi = taxcomApi ?? throw new ArgumentNullException(nameof(taxcomApi));
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
					_logger.LogInformation($"Пауза перед запуском транзакций {_delaySec}сек");
					await Task.Delay(_delaySec * 1000, stoppingToken);
					_logger.LogInformation("Запускаем необходимые транзакции");
					_taxcomApi.AutoSendReceive(stoppingToken);
				}
				catch(Exception e)
				{
					_logger.LogError(e, $"Ошибка при запуске {nameof(_taxcomApi.AutoSendReceive)}");
				}
			}
		}
	}
}
