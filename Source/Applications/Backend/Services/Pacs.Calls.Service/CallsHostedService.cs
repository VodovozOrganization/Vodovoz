using MassTransit;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Pacs.Calls.Service
{
	public class CallsHostedService : IHostedService
	{
		readonly IBusControl _bus;
		readonly ILogger _logger;

		public CallsHostedService(IBusControl bus, ILoggerFactory loggerFactory)
		{
			if(loggerFactory is null)
			{
				throw new ArgumentNullException(nameof(loggerFactory));
			}

			_bus = bus ?? throw new ArgumentNullException(nameof(bus));
			_logger = loggerFactory.CreateLogger<CallsHostedService>();
		}

		public async Task StartAsync(CancellationToken cancellationToken)
		{
			_logger.LogInformation("Сервис обработки событий звонков запущен");
			await _bus.StartAsync(cancellationToken).ConfigureAwait(false);
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			_logger.LogInformation("Сервис обработки событий звонков остановлен");
			return _bus.StopAsync(cancellationToken);
		}
	}
}
