using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Vodovoz.Infrastructure
{
	public abstract class TimerBackgroundServiceBase : BackgroundService
	{
		protected abstract TimeSpan Interval { get; }

		protected abstract void DoWork(CancellationToken stoppingToken);

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			OnStartService();
			while(!stoppingToken.IsCancellationRequested)
			{
				DoWork(stoppingToken);
				await Task.Delay(Interval, stoppingToken);
			}
		}

		public override Task StopAsync(CancellationToken cancellationToken)
		{
			OnStopService();
			return base.StopAsync(cancellationToken);
		}

		protected virtual void OnStartService()
		{
		}

		protected virtual void OnStopService()
		{
		}
	}
}
