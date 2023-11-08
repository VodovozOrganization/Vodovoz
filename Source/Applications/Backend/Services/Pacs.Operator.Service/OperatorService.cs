using MassTransit;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pacs.Operator.Service
{
	public class OperatorHostedService : IHostedService
	{
		readonly IBusControl _bus;
		readonly ILogger _logger;

		public OperatorHostedService(IBusControl bus, ILoggerFactory loggerFactory)
		{
			if(loggerFactory is null)
			{
				throw new ArgumentNullException(nameof(loggerFactory));
			}

			_bus = bus ?? throw new ArgumentNullException(nameof(bus));
			_logger = loggerFactory.CreateLogger<OperatorHostedService>();
		}

		public async Task StartAsync(CancellationToken cancellationToken)
		{
			_logger.LogInformation("Сервис операторов запущен");
			await _bus.StartAsync(cancellationToken).ConfigureAwait(false);
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			_logger.LogInformation("Сервис операторов остановлен");
			return _bus.StopAsync(cancellationToken);
		}
	}
}
