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
		private readonly IOptions<SyncOptions> _syncOptions;
		private readonly IServiceScopeFactory _serviceScopeFactory;
		private readonly ILogger<MangoStatsWorker> _logger;

		public MangoStatsWorker(
			IOptions<SyncOptions> syncOptions,
			IServiceScopeFactory serviceScopeFactory,
			ILogger<MangoStatsWorker> logger)
		{
			_syncOptions = syncOptions ?? throw new  ArgumentNullException(nameof(syncOptions));
			_serviceScopeFactory = serviceScopeFactory  ?? throw new  ArgumentNullException(nameof(serviceScopeFactory));
			_logger = logger ?? throw new  ArgumentNullException(nameof(logger));
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			_logger.LogInformation("CallSyncWorker started");

			while (!stoppingToken.IsCancellationRequested)
			{
				try
				{
					using var scope = _serviceScopeFactory.CreateScope();
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
