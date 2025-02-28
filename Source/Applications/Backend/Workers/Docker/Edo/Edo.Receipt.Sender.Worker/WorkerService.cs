using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ModulKassa;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Edo.Receipt.Sender.Worker
{
	public class WorkerService : BackgroundService
	{
		private readonly ILogger<WorkerService> _logger;
		private readonly CashboxClientProvider _cashboxClientProvider;
		private readonly IOptionsMonitor<CashboxesSetting> _options;

		public WorkerService(ILogger<WorkerService> logger, CashboxClientProvider cashboxClientProvider, IOptionsMonitor<CashboxesSetting> options)
		{
			_logger = logger;
			_cashboxClientProvider = cashboxClientProvider ?? throw new ArgumentNullException(nameof(cashboxClientProvider));
			_options = options ?? throw new ArgumentNullException(nameof(options));
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while(!stoppingToken.IsCancellationRequested)
			{
				var cashboxid = _options.CurrentValue.CashboxSettings.First().CashBoxId;
				_logger.LogInformation("Текущий Id первой кассы: {Id}", cashboxid);

				/*var cashbox = await _cashboxClientProvider.GetCashboxAsync(3, stoppingToken);
				_logger.LogInformation("Касса получена: {Id}", cashbox.Id);*/


				//_logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
				await Task.Delay(1000, stoppingToken);
			}
		}
	}
}
