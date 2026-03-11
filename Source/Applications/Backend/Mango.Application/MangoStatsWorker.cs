using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mango.Business.Interfaces;
using Mango.Contracts.V1.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Mango.Application
{
	public class MangoStatsWorker : BackgroundService
	{
		private readonly IServiceProvider _serviceProvider;
		private readonly IOptions<SyncOptions> _syncOptions;
		private readonly ILogger<MangoStatsWorker> _logger;

		public MangoStatsWorker(IServiceProvider serviceProvider,
			IOptions<SyncOptions> syncOptions,
			ILogger<MangoStatsWorker> logger)
		{
			_serviceProvider = serviceProvider;
			_syncOptions = syncOptions;
			_logger = logger;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			_logger.LogInformation("CallSyncWorker started");

			while (!stoppingToken.IsCancellationRequested)
			{
				try
				{
					using var scope = _serviceProvider.CreateScope();
					var syncService = scope.ServiceProvider.GetRequiredService<ICallStatisticService>();

					await syncService.LoadDataAsync(stoppingToken);
				}
				catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
				{
					_logger.LogInformation("CallSyncWorker work cancelled");
					break;
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Unhandled error during Mango sync");
				}

				await Task.Delay(
					TimeSpan.FromSeconds(_syncOptions.Value.PollIntervalSeconds),
					stoppingToken);
			}

			_logger.LogInformation("CallSyncWorker stopped");
		}
	}
}
