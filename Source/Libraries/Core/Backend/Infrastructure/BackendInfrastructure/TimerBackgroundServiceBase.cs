using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Vodovoz.Infrastructure
{
	public abstract class TimerBackgroundServiceBase : BackgroundService
	{
		protected abstract TimeSpan Interval { get; }

		protected abstract Task DoWork(CancellationToken stoppingToken);

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while(!stoppingToken.IsCancellationRequested)
			{
				await DoWork(stoppingToken);
				await Task.Delay(Interval, stoppingToken);
			}
		}

		public override async Task StartAsync(CancellationToken cancellationToken)
		{
			await base.StartAsync(cancellationToken);
		}

		public override async Task StopAsync(CancellationToken cancellationToken)
		{
			await base.StopAsync(cancellationToken);
		}
	}
}
