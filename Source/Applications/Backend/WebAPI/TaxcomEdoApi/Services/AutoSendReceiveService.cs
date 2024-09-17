using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Taxcom.Client.Api;
using TaxcomEdoApi.Library.Config;

namespace TaxcomEdoApi.Services
{
	public class AutoSendReceiveService : BackgroundService
	{
		private readonly ILogger<AutoSendReceiveService> _logger;
		private readonly EdoServicesOptions _edoServicesOptions;
		private readonly TaxcomApi _taxcomApi;

		public AutoSendReceiveService(
			ILogger<AutoSendReceiveService> logger,
			IOptions<EdoServicesOptions> edoServicesOptions,
			TaxcomApi taxcomApi)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_edoServicesOptions = (edoServicesOptions ?? throw new ArgumentNullException(nameof(edoServicesOptions))).Value;
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
					var delay = _edoServicesOptions.DelayBetweenAutoSendReceiveProcessingInSeconds;
					
					_logger.LogInformation("Пауза перед запуском транзакций {DelaySec}сек", delay);
					await Task.Delay(delay * 1000, stoppingToken);
					
					_logger.LogInformation("Запускаем необходимые транзакции");
					_taxcomApi.AutoSendReceive(stoppingToken);
				}
				catch(Exception e)
				{
					_logger.LogError(e, "Ошибка при запуске {AutoSendReceive}", nameof(_taxcomApi.AutoSendReceive));
				}
			}
		}
	}
}
