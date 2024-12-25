using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Edo.Scheduler.Worker
{
	public sealed class HostWarmUpService : IHostedService
	{
		private readonly ILogger _logger;
		private readonly IUnitOfWorkFactory _uowFactory;

		public HostWarmUpService(
			ILogger<HostWarmUpService> logger,
			IUnitOfWorkFactory uowFactory,
			IHostApplicationLifetime appLifetime)
		{
			_logger = logger;
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));

			appLifetime.ApplicationStarted.Register(OnStarted);
		}

		private void OnStarted()
		{
			_logger.LogInformation("Application starting");
			WarmUp();
			_logger.LogInformation("Application started");
		}

		public void WarmUp()
		{
			_uowFactory.CreateWithoutRoot().Dispose();
		}

		public Task StartAsync(CancellationToken cancellationToken)
		{
			return Task.CompletedTask;
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			return Task.CompletedTask;
		}
	}
}
