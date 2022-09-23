using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Vodovoz.Infrastructure
{
	/// <summary>
	/// База для сервиса, который должен периодически запускать действие
	/// </summary>
	public abstract class TimerServiceBase : IHostedService, IAsyncDisposable
	{
		private readonly Task _completedTask = Task.CompletedTask;
		private Timer? _timer;

		protected abstract void DoWork();
		protected abstract TimeSpan Interval { get; }

		public Task StartAsync(CancellationToken stoppingToken)
		{
			_timer = new Timer((state) => DoWork(), null, TimeSpan.Zero, Interval);
			OnStartService();
			return _completedTask;
		}

		protected virtual void OnStartService()
		{
		}

		public Task StopAsync(CancellationToken stoppingToken)
		{
			_timer?.Change(Timeout.Infinite, 0);
			OnStopService();
			return _completedTask;
		}

		protected virtual void OnStopService()
		{
		}

		public virtual async ValueTask DisposeAsync()
		{
			if(_timer is IAsyncDisposable timer)
			{
				await timer.DisposeAsync();
			}

			_timer = null;
		}
	}
}
