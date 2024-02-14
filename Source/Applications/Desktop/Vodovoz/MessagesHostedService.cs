using MassTransit;
using MassTransit.Transports;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pacs.Admin.Client;
using Pacs.Core;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Vodovoz
{
	public interface IMessageTransportInitializer
	{
		void Initialize(IBusControl bus);
	}

	public class MessagesHostedService : IHostedService, IMessageTransportInitializer
	{
		private IBusControl _bus;
		private readonly ILogger<MessagesHostedService> _logger;
		private bool _initialized = false;

		public MessagesHostedService(ILogger<MessagesHostedService> logger)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public void Initialize(IBusControl bus)
		{
			_bus = bus ?? throw new ArgumentNullException(nameof(bus));
			_initialized = true;
		}

		public async Task StartAsync(CancellationToken cancellationToken)
		{
			while(!_initialized)
			{
				await Task.Delay(200);
			}

			_logger.LogInformation("Сервис сообщений запущен");

			try
			{
				await _bus.StartAsync(cancellationToken).ConfigureAwait(false);
			}
			catch(PacsInitException ex)
			{
				_logger.LogInformation(ex.Message);
			}
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			_logger.LogInformation("Сервис сообщений остановлен");
			return _bus.StopAsync(cancellationToken);
		}
	}
}
