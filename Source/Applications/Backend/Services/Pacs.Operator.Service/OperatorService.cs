using MassTransit;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pacs.Operators.Service
{
	public class OperatorHostedService : IHostedService
	{
		readonly IBusControl _busControl;
		private readonly ILogger<OperatorHostedService> _logger;

		public OperatorHostedService(IBusControl busControl, ILogger<OperatorHostedService> logger)
		{
			_busControl = busControl ?? throw new ArgumentNullException(nameof(busControl));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public async Task StartAsync(CancellationToken cancellationToken)
		{
			_logger.LogInformation("Публикация топологии");
			//await _busControl.DeployAsync();
		}

		public async Task StopAsync(CancellationToken cancellationToken)
		{
			await Task.CompletedTask;
		}
	}
}
