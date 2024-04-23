using Autofac;
using MassTransit;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pacs.Core;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Vodovoz
{
	public interface IMessageTransportInitializer
	{
		void Initialize();
	}

	public class MessagesHostedService : IHostedService, IMessageTransportInitializer
	{
		private IBusControl _bus;
		private readonly ILogger<MessagesHostedService> _logger;
		private readonly ILifetimeScope _scope;
		private bool _initialized = false;

		public MessagesHostedService(ILogger<MessagesHostedService> logger, ILifetimeScope scope)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_scope = scope?.BeginLifetimeScope() ?? throw new ArgumentNullException(nameof(scope));
		}

		public void Initialize()
		{
			_bus = _scope.Resolve<IBusControl>();
			_initialized = true;
		}

		public async Task StartAsync(CancellationToken cancellationToken)
		{
			while(!_initialized && !cancellationToken.IsCancellationRequested)
			{
				await Task.Delay(200);
			}

			if(cancellationToken.IsCancellationRequested)
			{
				_logger.LogInformation("Запрошено завершение сервиса");
				return;
			}

			try
			{
				await _bus.StartAsync(cancellationToken).ConfigureAwait(false);
			}
			catch(PacsInitException ex)
			{
				_logger.LogInformation(ex.Message);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка при подключении к шине сообщений");
			}
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			_logger.LogInformation("Сервис сообщений остановлен");
			_scope.Dispose();
			return _bus?.StopAsync(cancellationToken);
		}
	}
}
